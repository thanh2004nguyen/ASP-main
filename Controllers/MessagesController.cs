using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group5.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Group5.Hubs;
using Group5.ViewModels;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using System.Net.Http.Headers;
using Group5.Service;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using RestSharp;
using Group5.Data;
using System.Security.Claims;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Group5.Controllers
{
    /*  [Authorize]*/
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        IMessageService service;
        IRequestService requestService;
        ICustomRoleService customRoleService;


        public MessagesController(ApplicationDbContext context, IMessageService service,
            IMapper mapper,
            IHubContext<ChatHub> hubContext, IRequestService requestService, 
            ICustomRoleService customRoleService)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            this.service = service;
            this.requestService = requestService;
            this.customRoleService = customRoleService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            var messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
            return Ok(messageViewModel);
        }

        [HttpGet("Room/{roomName}")]
        public IActionResult GetMessages(string roomName)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Name == roomName);
            if (room == null)
                return BadRequest();

            var messages = _context.Messages.Where(m => m.ToRoomId == room.Id)
                .Include(m => m.FromUser)
                .Include(m => m.ToRoom)
                .OrderByDescending(m => m.Timestamp)
                .Take(20)
                .AsEnumerable()
                .Reverse()
                .ToList();

            foreach (var message in messages)
            {
                message.Seen = true;
                _context.Update(message);
                _context.SaveChanges();
            }

            var messagesViewModel = _mapper.Map<IEnumerable<Message>, IEnumerable<MessageViewModel>>(messages);

            return Ok(messagesViewModel);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(MessageViewModel viewModel)
        {

            var LoginEmail = "";
            if (User.Identity!.IsAuthenticated)
            {
                LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }


            var user = _context.Users.FirstOrDefault(u => u.Email == LoginEmail);

          
            var admin = _context.Users.FirstOrDefault(u => u.Email == "admin@gmail.com");
            
            var room = _context.Rooms.FirstOrDefault(r => r.Name == viewModel.Room);
            if (room == null)
                return BadRequest();
            string textcontent = Regex.Replace(viewModel!.Content!, @"<.*?>", string.Empty);

            var msg = new Message()
            {
                Content = await service.CheckSpelling1(textcontent),
                FromUser = user,
                ToRoom = room,
                Timestamp = DateTime.Now,
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Broadcast the message
            var createdMessage = _mapper.Map<Message, MessageViewModel>(msg);
            await _hubContext.Clients.Group(room!.Name!).SendAsync("newMessage", createdMessage);
            var replycontent = "";
            if (Regex.IsMatch(msg.Content, @"\bopen\b.*\bhour(s)?\b|\bhour(s)?\b.*\bopen\b", RegexOptions.IgnoreCase))
            {
                replycontent = "Open Hour Is 8:00AM And Close At 17:00PM";
            }
            else if (Regex.IsMatch(msg.Content, @"\brequest\b.*\bId(s)?\b|\bId(s)?\b.*\brequest\b", RegexOptions.IgnoreCase))
            {
                var requestIdMatch = Regex.Match(msg.Content, @"\b(\d+)\b");

                if (requestIdMatch.Success)
                {
                    int requestId = int.Parse(requestIdMatch.Groups[1].Value);
                    var request = await requestService.FindRequestById(requestId);
                    if (request != null)
                    {
                        if (request!.RequestBy!.Id == user!.Id)
                        {
                            string status = request!.RequestStatus!.Status!;
                            string statusColor = "red";
                            string formattedStatus = $"<span style='color:{statusColor}'>{status}</span>";
                            string baseUrl = "http://localhost:5240";
                            string requestDetailUrl = $"{baseUrl}/Request/OwnRequestDetail/{requestId}";
                            string clickableUrl = $"<a href='{requestDetailUrl}'>{requestDetailUrl}</a>";
                            replycontent = $"Your Request Detail Is: {formattedStatus}. Click {clickableUrl} for more details.";
                        }
                        else
                        {
                            replycontent = "Sorry!! Please Input Valid Request ID";
                        }
                    }
                    else
                    {
                        var Newrequest = await requestService.FindNewRequestById(requestId);
                        if (Newrequest != null)
                        {
                            if (Newrequest!.RequestBy!.Id == user!.Id)
                            {
                                string status = Newrequest!.RequestStatus!.Status!;
                                string statusColor = "red";
                                string formattedStatus = $"<span style='color:{statusColor}'>{status}</span>";
                                string baseUrl = "http://localhost:5240";
                                string requestDetailUrl = $"{baseUrl}/Request/OwnNewRequestDetail/{requestId}";
                                string clickableUrl = $"<a href='{requestDetailUrl}'>{requestDetailUrl}</a>";
                                replycontent = $"Your Request Detail Is: {formattedStatus}. Click {clickableUrl} for more details.";
                            }
                            else
                            {
                                replycontent = "Sorry!! Please Input Valid Request ID";
                            }
                        }
                        else replycontent = "Sorry!! Please Input Valid Request ID";
                    }

                }
            }

            else if (Regex.IsMatch(msg.Content, @"\blist\b.*\brequest(s)?\b|\brequest(s)?\b.*\blist\b", RegexOptions.IgnoreCase))
            {
                var listRequest = await requestService.ListRequest(user!);

                if (listRequest.Any())
                {
                    // Create a comma-separated string of request IDs
                    string requestIds = string.Join(", ", listRequest);

                    replycontent = $"Your Request IDs: {requestIds}. You can use Sytax [request id + Request ID Number] To Know More Detail";
                }
                else
                {
                    replycontent = "You don't have any requests.";
                }
            }

            else if (Regex.IsMatch(msg.Content, @"\bchange\b.*\bpass(word)?\b|\bpass(word)?\b.*\bchange\b", RegexOptions.IgnoreCase))
            {

                replycontent = "You can go Profile Detail Page to change Password.And you need Old Password.Incase You dont renember password , Please contact manager!";

            }

            else if (Regex.IsMatch(msg.Content, @"\bsytax\b", RegexOptions.IgnoreCase))
            {
                string statusColor1 = "red";
                string sytax1 = "1. [list request] to get list all your submited Request.";
                string formattedStatus1 = $"<div style='color:{statusColor1}'>{sytax1}</div>";

                string statusColor2 = "blue";
                string sytax2 = "2. [request id + request-id-number] To get detail of Request#request-id-number";
                string formattedStatus2 = $"<div style='color:{statusColor2}'>{sytax2}</div>";

                string sytax3 = "3. [change password] Show How to Change Password";
                string formattedStatus3 = $"<div style='color:{statusColor1}'>{sytax3}</div>";
                string sytax4 = "4. [profile] Show Your Profile";
                string formattedStatus4 = $"<div style='color:{statusColor2}'>{sytax4}</div>";

                replycontent = $"Some UseFull Sytax: {formattedStatus1}{formattedStatus2}{formattedStatus3}{formattedStatus4}";
            }

            else if (Regex.IsMatch(msg.Content, @"\bprofile\b", RegexOptions.IgnoreCase))
            {
                var loginEmail = "";
                if (User?.Identity?.IsAuthenticated == true)
                {
                    loginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                var userlogin = await _context.Users
                    .Include(a => a.Departments)
                    .Include(a => a.EmployeePositions)
                    .FirstOrDefaultAsync(a => a.Email == loginEmail);

                string status = userlogin!.FullName!;
                float? amount = userlogin.AmountRequestPerMonth;
                string statusColor = "red";
                string formattedStatus = $"<span style='color:{statusColor}'>{status}</span>";
                string formattedAmount = $"<span style='color:{statusColor}'>{amount}</span>";
                string baseUrl = "http://localhost:5240";
                string userdetail = $"{baseUrl}/User/Detail";
                string clickableUrl = $"<a href='{userdetail}'>{userdetail}</a>";
                replycontent = $"Hello: {formattedStatus}.You have ${formattedAmount} Request Amount. Click {clickableUrl} for more details.";
            }


            if (replycontent!="")
            {
                var repmsg = new Message()
                {
                    Content = replycontent,
                    FromUser = admin,
                    ToRoom = room,
                    Timestamp = DateTime.Now
                };
                _context.Messages.Add(repmsg);
                await _context.SaveChangesAsync();
                var createdMessage1 = _mapper.Map<Message, MessageViewModel>(repmsg);
                await _hubContext.Clients.Group(room.Name!).SendAsync("newMessage", createdMessage1);

            }
            

             
                
            return CreatedAtAction(nameof(Get), new { id = msg.Id }, createdMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.Messages
                .Include(u => u.FromUser)
               /* .Where(m => m.Id == id && m.FromUser.FullName == "")*/
                .FirstOrDefaultAsync();

            if (message == null)
                return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatMessage", message.Id);

            return Ok();
        }
    }
}

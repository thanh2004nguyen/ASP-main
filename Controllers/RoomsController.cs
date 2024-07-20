using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Group5.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Group5.Hubs;
using Microsoft.AspNetCore.SignalR;
using Group5.ViewModels;
using System.Text.RegularExpressions;
using Group5.Data;
using System.Security.Claims;
using Group5.Service;

namespace Group5.Controllers
{
    /*   [Authorize]*/
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ICustomRoleService customRoleService;

        public RoomsController(ApplicationDbContext context,
            IMapper mapper,
            IHubContext<ChatHub> hubContext, ICustomRoleService customRoleService)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
            this.customRoleService = customRoleService;
        }

        [HttpGet]
        [Route("list")]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> List()
        {
            List<Room> rooms;
            rooms = await _context.Rooms
              .Include(r => r.Admin)
              .ToListAsync();
           
            var roomsViewModel = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(rooms);
            return Ok(roomsViewModel);
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomViewModel>>> Get()
        {
            List<Room> rooms;
            var LoginEmail = "";
            if (User.Identity.IsAuthenticated)
            {
                LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewAdminChat"))
            {
                rooms = await _context.Rooms
            .Include(r => r.Admin)
            .ToListAsync();

            }
            else
            {
                rooms = await _context.Rooms
                    .Include(r => r.Admin)
                    .Where(a => a.Admin.Email == LoginEmail)
                    .ToListAsync();
            }
            var roomsViewModel = _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(rooms);
            return Ok(roomsViewModel);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            var roomViewModel = _mapper.Map<Room, RoomViewModel>(room);
            return Ok(roomViewModel);
        }

        [HttpPost]
        public async Task<ActionResult<Room>> Create(RoomViewModel viewModel)
        {
       
            var LoginEmail = "";
            if (User.Identity.IsAuthenticated)
            {
                LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            if (_context.Rooms.Any(r => r.Name == viewModel.Name))
                return BadRequest("Invalid room name or room already exists");


            var user = _context.Users.FirstOrDefault(u => u.Email == LoginEmail);
         
            if (_context.Rooms.Any(r => r.Name == LoginEmail))
            {
                return null;
            }
            else
            {
                var room = new Room()
                {
                    Name = LoginEmail,
                    Admin = user
                };
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                var createdRoom = _mapper.Map<Room, RoomViewModel>(room);
                await _hubContext.Clients.All.SendAsync("addChatRoom", createdRoom);


                var accAdmin = _context.Users.FirstOrDefault(u => u.Email == "admin@gmail.com");
                var msg = new Message()
                {
                    Content = "Welcome To Support Area,Please Type 'sytax' To Know Some UseFull Sytax For Chat Bot ",
                    FromUser = accAdmin,
                    ToRoom = room,
                    Timestamp = DateTime.Now
                };

                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();


                return CreatedAtAction(nameof(Get), new { id = room.Id }, createdRoom);

            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, RoomViewModel viewModel)
        {
            if (_context.Rooms.Any(r => r.Name == viewModel.Name))
                return BadRequest("Invalid room name or room already exists");

            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.Email == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            room.Name = viewModel.Name;
            await _context.SaveChangesAsync();

            var updatedRoom = _mapper.Map<Room, RoomViewModel>(room);
            await _hubContext.Clients.All.SendAsync("updateChatRoom", updatedRoom);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Admin)
                .Where(r => r.Id == id && r.Admin.Email == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("removeChatRoom", room.Id);
            await _hubContext.Clients.Group(room.Name).SendAsync("onRoomDeleted");

            return Ok();
        }
    }
}

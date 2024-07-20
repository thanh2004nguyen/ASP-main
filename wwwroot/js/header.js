$(document).ready(function () {
    $(".delete-cart-item-head").click(function (e) {
        console.log("aaaaaaaa");
        e.preventDefault();
        var itemId = $(this).data("id");
        var row = $("#cartItem_" + itemId);

        $.ajax({
            url: "/Cart/DeleteCartItem",
            type: "GET",
            data: { id: itemId },
            success: function (data) {
                if (data.success) {
                    row.fadeOut(1, function () {
                        row.remove();
                    });
                    console.log("aaaaaaaa");
                    $("#cartcount").html(data.items);
                } else {
                    console.error("Error deleting cart item:", data.error);
                }
            },
            error: function (error) {
                // Handle errors
                console.error("Error deleting cart item:", error);
            }
        });
    });

});
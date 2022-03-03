
$(function () {
    console.log("Page is ready");

    $(document).on("click", ".square-button", function () {
        event.preventDefault();

        var squareLoc = $(this).val();
        console.log("you clicked on: " + squareLoc);
        doSquareUpdate(squareLoc);
    })

    function doSquareUpdate(squareLoc) {
        $.ajax({
            //dataType: "json",
            method: "POST",
            url: "/Home/UpdateChangedSquares",
            data: {
                "location": squareLoc
            },
            error: function (jqXHR) {
                console.log(jqXHR.statusText);
                console.log(jqXHR);
            },
            success: function (data) {
                console.log(data);
                $("#" + squareLoc).html(data);
            }
        })
    }
});
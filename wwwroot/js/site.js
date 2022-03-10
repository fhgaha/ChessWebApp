
$(function () {
    console.log("Page is ready");

    $(document).on("click", ".square-button", function () {
        event.preventDefault();

        var squareLoc = $(this).val();
        var greenText = "color: #bada55";
        console.log("%c you clicked on: " + squareLoc, greenText);
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
                //console.log(data);
                //$("#" + squareLoc).html(data);

                for (var d in data) {
                    console.log(data[d]);
                    $("#" + d).html(data[d]);
                }
            }
        })
    }
});
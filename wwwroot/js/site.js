const green = "color: #bada55";
const red = "color: #da5555";

$(function () {
    console.log("Page is ready");

    $(document).on("click", ".square-button", function () {
        event.preventDefault();

        var squareLoc = $(this).val();
        console.log("%c you clicked on: " + squareLoc, green);

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
                console.log(jqXHR.statusText, red);
                console.log(jqXHR, red);
            },
            success: function (data) {
                //console.dir(data);
                $("#" + squareLoc).html(data);

                TryOpenPromotionModal(squareLoc);

                for (var d in data) {
                    //console.log(data[d]);
                    $("#" + d).html(data[d]);
                }
            }
        })
    }

    function TryOpenPromotionModal(squareLoc) {
        $.ajax({
            type: "json",
            data: {
                "location": squareLoc
            },
            url: "/Home/CheckPromotionJSON",
            success: function (data) {
                console.log("Square data:");
                console.log(data);

                if (data != null && data.name == "Pawn"
                    && (data.location.rank == 8 && data.pieceColor == 0
                     || data.location.rank == 1 && data.pieceColor == 1 )) {

                    if (data.pieceColor == "0")
                        $('#WhitePiecesPromotionModal').modal('show');
                    else if (data.pieceColor == "1")
                        $('#BlackPiecesPromotionModal').modal('show');
                }
            }
        })
    }

    $(document).on("click", ".promotion-button", function () {
        var pieceData = $(this).val();    //"Rook white A 1"
        console.log("You clicked on promotion button " + pieceData);

        $.ajax({
            type: "text",
            data: {
                "pieceData": pieceData
            },
            url: "/Home/PromotePawn",
            success: function (data) {
                console.log(data);

                //need to update squares on view
                for (var d in data) {
                    //console.log(data[d]);
                    $("#" + d).html(data[d]);
                }
            }
        })
    })
});
const green = "color: #bada55";
const red = "color: #da5555";

$(document).ready(function () {

    console.log("Page is ready");

    $(document).on("click", ".square-button", function () {
        event.preventDefault();

        var squareLoc = $(this).val();
        console.log("%c you clicked on: " + squareLoc, green);

        doSquaresUpdate(squareLoc);
    })

    $(document).on("click", ".promotion-button", function () {
        event.preventDefault();

        var pieceData = $(this).val();    //"Rook white A 1"
        //console.log("You clicked on promotion button " + pieceData);

        $.ajax({
            type: "text",
            data: {
                "pieceData": pieceData
            },
            url: "/Home/PromotePawnJSON",
            success: function (data) {
                console.log(data);

                for (var d in data) {
                    //console.log(data[d]);
                    $("#" + d).html(data[d]);
                }
            }
        })
    })

    //FEN
    $(document).on("submit", ".fen-zone", function () {
        event.preventDefault();

        var fenValue = $('#fen-input').val();

        //update fen input 
        //and set the board

        $('fen-input').val(fenValue);

        $.ajax({
            type: "text",
            data: {
                "fenCode": fenValue
            },
            url: "/Home/SetFenJSON",
            success: function (data) {
                //console.log(data);

                for (var d in data) {
                    //console.log(data[d]);
                    $("#" + d).html(data[d]);
                }
            }
        })

    })

    //changing main-grid cells width to match board size
    //for some reason this can make <html> more narrow than main-grid. lichess has the same issue though.
    $('.main_grid').each(function () { //loop through each element with the .main_grid class
        $(this).css({
            'grid-template-columns': $('.board-zone').outerWidth() + 'px', //adjust the css rule for margin-top to equal the element height - 10px and add the measurement unit "px" for valid CSS
        });
    });

    //left arrow key
    $(document).keydown(function (e) {
        if (e.which == 37) {
            console.log("left pressed");

            $.ajax({
                type: "text",
                url: "/Home/UndoMove",
                success: function (data) {
                    console.log(data);

                    for (var d in data) {
                        //console.log(data[d]);
                        $("#" + d).html(data[d]);
                    }
                }
            })
            return false;
        }
    });

    //right arrow key
    $(document).keydown(function (e) {
        if (e.which == 39) {
            console.log("right pressed");

            //$.ajax({
            //    type: "text",
            //    url: "/Home/UndoMove",
            //    success: function (data) {
            //        console.log(data);

            //        for (var d in data) {
            //            //console.log(data[d]);
            //            $("#" + d).html(data[d]);
            //        }
            //    }
            //})
            return false;
        }
    });
});

function doSquaresUpdate(squareLoc) {
    $.ajax({
        //dataType: "json",
        method: "POST",
        url: "/Home/UpdateChangedSquaresJSON",
        data: { "location": squareLoc },
        //async: false,
        success: function (data) {
            console.log("doSquareUpdate", green);
            //console.dir(data);
            $("#" + squareLoc).html(data);

            TryOpenPromotionModal(squareLoc);

            for (var d in data) {
                //console.log(data[d]);
                $("#" + d).html(data[d]);
            }

            setTimeout(function () {
                GetPlayer();
            }, 0);
        }
    });
}

function TryOpenPromotionModal(squareLoc) {
    $.ajax({
        type: "json",
        data: { "location": squareLoc },
        url: "/Home/CheckPromotionJSON",
        success: function (data) {
            //console.log("Square data:");
            //console.log(data);

            if (data != null && data.name == "Pawn"
                && (data.location.rank == 8 && data.pieceColor == 0
                    || data.location.rank == 1 && data.pieceColor == 1)) {

                if (data.pieceColor == "0")
                    $('#WhitePromotionModal').modal('show');
                else if (data.pieceColor == "1")
                    $('#BlackPromotionModal').modal('show');
            }
        }
    })
}

function doFenUpdate() {
    $.ajax({
        url: "/Home/GetFen",
        async: false,
        success: function (data) {
            console.log("doFenUpdate");
            $('#fen-input').val(data);
        }
    });
}

//if player is machine let it make a move
//view should not update until controller returns ready
function GetPlayer() {
    $.ajax({
        url: "/Home/GetPlayer",
        async: false,
        success: function (data) {
            console.log("GetPlayer: ", data);
            if (data == "ready") {
                doFenUpdate();
                doSquaresUpdate(null);
            }
        }
    });
}

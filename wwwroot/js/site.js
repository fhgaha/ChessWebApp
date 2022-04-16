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
            url: "/Home/UpdateChangedSquaresJSON",
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
                        $('#WhitePromotionModal').modal('show');
                    else if (data.pieceColor == "1")
                        $('#BlackPromotionModal').modal('show');
                }
            }
        })
    }

    $(document).on("click", ".promotion-button", function () {
        event.preventDefault();

        var pieceData = $(this).val();    //"Rook white A 1"
        console.log("You clicked on promotion button " + pieceData);

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

    
});



function toggle() {
    //event.preventDefault();
    var styleSheet = document.getElementById("dynamicCss");
    //var oldHref = styleSheet.getAttribute('href');

    var oldHref = window.localStorage.getItem('currentTheme');

    var newHref = oldHref == "/css/site-light.css"
        ? '/css/site-dark.css' : '/css/site-light.css';

    //styleSheet.setAttribute('href', newHref);

    const oldStyleSheet = $('#dynamicCss');
    const newStyleSheet = oldStyleSheet.clone();
    //oldStyleSheet.attr('id', 'old-theme');
    newStyleSheet.attr('href', newHref);
    ts.after(newStyleSheet);
    setTimeout(() => {
        $('#dynamicCss').remove()
    }, 300);

    window.localStorage.setItem('currentTheme', newHref);
}




//const themeName = themeSelector.value;
//$('#theme-styles').attr('href', `/bootswatch/dist/${themeName}/bootstrap.min.css`);

const handleThemeSelectorChange = (s, e) => {
    const themeName = themeSelector.value;
    const href = `/bootswatch/dist/${themeName}/bootstrap.min.css`;

    // Changing the href of a stylesheet link immediately takes that stylesheet out 
    // of the set of styles and the default styles will be applied in the interval 
    // between the new href and the old href. The effect is a noticeable flicker as 
    // elements change size, shape, and color. We can work around this by creating a
    // new stylesheet with the new href and sliding it below the original 
    // stylesheet. We then give it a just a few milliseconds to apply the new styles
    // before removing the original stylesheet. If we change the theme rapid fire, 
    // we could wind up with several old stylesheets for just a beat. But we're 
    // using jQuery to remove them all, leaving only the new one. The effect is a 
    // much smoother transition between themes.
    const ts = $('#dynamicCss');
    const ss = ts.clone();
    ts.attr('id', 'old-theme');
    ss.attr('href', href);
    ts.after(ss);
    setTimeout(() => {
        $('#old-theme').remove()
    }, 300)
    window.localStorage.setItem('currentTheme', themeName);
}


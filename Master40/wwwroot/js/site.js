// Write your Javascript code.
//function to grey out the screen
$(function () {
    // Create overlay and append to body:
    $('<div id="ajax-busy" style="z-index: 99999">' +
        '<div class="modal-dialog">' + 
            '<div class="modal-content">' +
                '<div class="modal-header">' +
                    '<h4 class="modal-title">Processing</h4>' +
                '</div>' +
                '<div class="modal-body">' + 
                    '<div class="row">'+ 
                    '<div class="MessageDisplayRunning" style="width: 100%;padding-left: 140px; float: right; overflow-y: auto; max-height: 400px"></div>' +
                    '<div class="loading" style="width: 120px;margin 5px"></div>' +
                    '</div>' +
                '</div>'+
            '</div>' +
        '</div></div>').css({
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100%',
        height: ($(window).height() + 20) + 'px',
        background: 'rgba(255,255,255,0.8)'
    }).hide().appendTo('body');
});

imagePreview = function () {
    /* CONFIG */

    var xOffset = 10;
    var yOffset = 30;

    // these 2 variable determine popup's distance from the cursor
    // you might want to adjust to get the right result

    /* END CONFIG */
    $("a.preview").hover(function (e) {
            this.t = this.title;
            this.title = "";
            var c = (this.t !== "") ? "<br/>" + this.t : "";
            $("body").append("<p id='preview'><img src='" + this.href + "' alt='Image preview' width='800px'/>" + c + "</p>");
            $("#preview")
                .css("top", (e.pageY - xOffset) + "px")
                .css("left", (e.pageX + yOffset - 800) + "px")
                .fadeIn("fast");
        },
        function () {
            this.title = this.t;
            $("#preview").remove();
        });
    $("a.preview").mousemove(function (e) {
        $("#preview")
            .css("top", (e.pageY - xOffset) + "px")
            .css("left", (e.pageX - yOffset - 800) + "px");
    });
};

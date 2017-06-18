// Write your Javascript code.
//function to grey out the screen
$(function () {
    // Create overlay and append to body:
    $('<div id="ajax-busy" style="z-index: 99999">' +
        '<div id="ContentBlock">' +
        '<div style="width: 344px; display: inline-block;">' +
            '<h4>&nbsp;Processing State: </h4>' +
                '<div class="MessageDisplayRunning" style="float: left; width: 200px; margin: 5px; overflow-y: auto; max-height: 130px;"></div>' +
                    '<div class="loading" style="float: left; width: 120px; margin: 5px"></div>' +
                '</div>'+
            '</div>' +
        '</div>').css({
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100%',
        height: $(window).height() + 'px',
        background: 'rgba(255,255,255,0.8)'
    }).hide().appendTo('body');
});

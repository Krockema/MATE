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
                    '<div class="MessageDisplayRunning" style="width: 100%;padding-left: 140px; float: right""></div>' +
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

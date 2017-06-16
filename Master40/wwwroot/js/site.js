// Write your Javascript code.
//function to grey out the screen
$(function () {
    // Create overlay and append to body:
    $('<div id="ajax-busy"/>').css({
        opacity: 0.8,
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100%',
        height: $(window).height() + 'px',
        background: 'white url(/images/loading.svg) no-repeat center'
    }).hide().appendTo('body');
});

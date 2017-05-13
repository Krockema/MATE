// Write your Javascript code.
var timeline;
$(document).ready(timeline_init);

function timeline_init() {
    var options = {
        phases: [
            { start: -1, end: 0, indicatorsEvery: 1, share: .12, className: 'name-phase' },
            { start: 1, end: 20, indicatorsEvery: 1, share: .3 },
        ],
        formatHeader: function (v) {
            if (v < 0) return 'Machines';
            if (v > 0) return v + ' ';
            return ' ';
        },
        barHeight: 25,
        fontSize: 16
    };

    // note: item 'Lonely' has a popup text attached
    data = @Html.Raw(Model);
    timeline = $('#timeline');
    timeline.simpleTimeline(options, data);
    timeline.on('timeline:barclick', timeline_clicked);
}

function timeline_clicked(e) {
    var clicked_item = $(e.target);

    var sel = $('.selected');
    sel.removeClass('selected');
    $('#clicked-item').empty();

    if (sel.length === 0 || sel.data('id') !== clicked_item.data('id')) {
        clicked_item.addClass('selected');
        $('#clicked-item').text("You clicked " + clicked_item.data('id'));
    }
}

function add_item() {
    var data = timeline.getTimelineData();
    data.push([
        {
            id: 'New One',
            start: 10,
            end: 15,
            className: 'styleA'
        }
    ]);
    timeline.setTimelineData(data).refreshTimeline();
}

function remove_selected_item() {
    var sel_item_id = $('.selected').data('id');
    var data = timeline.getTimelineData();
    for (var l = 0; l < data.length; l++) {
        for (var i = 0; i < data[l].length; i++) {
            if (data[l][i].id === sel_item_id) {
                data[l].splice(i, 1);
                if (data[l].length === 0)
                    data.splice(l, 1);
                timeline.setTimelineData(data).refreshTimeline();
                return;
            }
        }
    }
}

function bind_popup() {
    var sel_item_id = $('.selected').data('id');
    if (typeof sel_item_id === 'undefined') {
        alert('Ain\'t nothin\' selected, yo!');
        return;
    }

    timeline.bindPopup(sel_item_id, '<p><b>Yo it\'s great</b></p><p>Lorem ipsum dolizzle da bomb da bomb, consectetuer adipiscing elit. Nullam sapien velit, boom shackalack volutpizzle, suscipizzle daahng dawg, gravida vel, hizzle. Pellentesque go to hizzle tortor. Sed eros. Izzle at dolizzle dapibus turpis tempizzle gangster. Maurizzle fo shizzle my nizzle nibh et turpizzle. Owned in tortizzle. Pellentesque away rhoncizzle nizzle.</p><p>For sure bizzle massa go to hizzle shizzlin dizzle. Boom shackalack tellivizzle ipsum primis in crunk gangster luctizzle et stuff yo mamma izzle Break yo neck, yall; Nizzle sure. Pellentesque stuff check out this get down get down senectizzle et netizzle bow wow wow malesuada fizzle ac gangster egestas. Funky fresh tempor cool crackalackin. Fizzle erizzle mah nizzle.</p>');
}

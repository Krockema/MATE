Chart.defaults.RoundedDoughnut = Chart.helpers.clone(Chart.defaults.doughnut);
Chart.controllers.RoundedDoughnut = Chart.controllers.doughnut.extend({
    draw: function (ease) {
        var ctx = this.chart.chart.ctx;

        var easingDecimal = ease || 1;
        Chart.helpers.each(this.getMeta().data, function (arc, index) {
            arc.transition(easingDecimal).draw();

            var vm = arc._view;
            var radius = (vm.outerRadius + vm.innerRadius) / 2;
            var thickness = (vm.outerRadius - vm.innerRadius) / 2;
            var angle = Math.PI - vm.endAngle - Math.PI / 2;

            ctx.save();
            ctx.fillStyle = vm.backgroundColor;
            ctx.translate(vm.x, vm.y);
            ctx.beginPath();
            ctx.arc(radius * Math.sin(angle), radius * Math.cos(angle), thickness, 0, 2 * Math.PI);
            ctx.arc(radius * Math.sin(Math.PI), radius * Math.cos(Math.PI), thickness, 0, 2 * Math.PI);
            ctx.closePath();
            ctx.fill();
            ctx.restore();
        });
    },
});
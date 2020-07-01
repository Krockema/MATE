(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports, require('chart.js')) :
        typeof define === 'function' && define.amd ? define(['exports', 'chart.js'], factory) :
            (global = global || self, factory(global.ChartBoxPlot = {}, global.Chart));
}(this, function (exports, Chart) {
    'use strict';

    function _defineProperty(obj, key, value) {
        if (key in obj) {
            Object.defineProperty(obj, key, {
                value: value,
                enumerable: true,
                configurable: true,
                writable: true
            });
        } else {
            obj[key] = value;
        }

        return obj;
    }

    function _objectSpread(target) {
        for (var i = 1; i < arguments.length; i++) {
            var source = arguments[i] != null ? arguments[i] : {};
            var ownKeys = Object.keys(source);

            if (typeof Object.getOwnPropertySymbols === 'function') {
                ownKeys = ownKeys.concat(Object.getOwnPropertySymbols(source).filter(function (sym) {
                    return Object.getOwnPropertyDescriptor(source, sym).enumerable;
                }));
            }

            ownKeys.forEach(function (key) {
                _defineProperty(target, key, source[key]);
            });
        }

        return target;
    }

    function _slicedToArray(arr, i) {
        return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _nonIterableRest();
    }

    function _arrayWithHoles(arr) {
        if (Array.isArray(arr)) return arr;
    }

    function _iterableToArrayLimit(arr, i) {
        var _arr = [];
        var _n = true;
        var _d = false;
        var _e = undefined;

        try {
            for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) {
                _arr.push(_s.value);

                if (i && _arr.length === i) break;
            }
        } catch (err) {
            _d = true;
            _e = err;
        } finally {
            try {
                if (!_n && _i["return"] != null) _i["return"]();
            } finally {
                if (_d) throw _e;
            }
        }

        return _arr;
    }

    function _nonIterableRest() {
        throw new TypeError("Invalid attempt to destructure non-iterable instance");
    }

    function ascending(a, b) {
        return a - b;
    }

    function quantiles(d, quantiles) {
        d = d.slice().sort(ascending);
        var n_1 = d.length - 1;
        return quantiles.map(function (q) {
            if (q === 0) return d[0]; else if (q === 1) return d[n_1];
            var index = 1 + q * n_1,
                lo = Math.floor(index),
                h = index - lo,
                a = d[lo - 1];
            return h === 0 ? a : a + h * (d[lo] - a);
        });
    }

    // See <http://en.wikipedia.org/wiki/Kernel_(statistics)>.
    function gaussian(u) {
        return 1 / Math.sqrt(2 * Math.PI) * Math.exp(-.5 * u * u);
    }

    // Welford's algorithm.
    function mean(x) {
        var n = x.length;
        if (n === 0) return NaN;
        var m = 0,
            i = -1;

        while (++i < n) m += (x[i] - m) / (i + 1);

        return m;
    }

    // Also known as the sample variance, where the denominator is n - 1.

    function variance(x) {
        var n = x.length;
        if (n < 1) return NaN;
        if (n === 1) return 0;
        var mean$$1 = mean(x),
            i = -1,
            s = 0;

        while (++i < n) {
            var v = x[i] - mean$$1;
            s += v * v;
        }

        return s / (n - 1);
    }

    function iqr(x) {
        var quartiles = quantiles(x, [.25, .75]);
        return quartiles[1] - quartiles[0];
    }

    // Visualization. Wiley.

    function nrd(x) {
        var h = iqr(x) / 1.34;
        return 1.06 * Math.min(Math.sqrt(variance(x)), h) * Math.pow(x.length, -1 / 5);
    }

    function functor(v) {
        return typeof v === "function" ? v : function () {
            return v;
        };
    }

    function kde() {
        var kernel = gaussian,
            sample = [],
            bandwidth = nrd;

        function kde(points, i) {
            var bw = bandwidth.call(this, sample);
            return points.map(function (x) {
                var i = -1,
                    y = 0,
                    n = sample.length;

                while (++i < n) {
                    y += kernel((x - sample[i]) / bw);
                }

                return [x, y / bw / n];
            });
        }

        kde.kernel = function (x) {
            if (!arguments.length) return kernel;
            kernel = x;
            return kde;
        };

        kde.sample = function (x) {
            if (!arguments.length) return sample;
            sample = x;
            return kde;
        };

        kde.bandwidth = function (x) {
            if (!arguments.length) return bandwidth;
            bandwidth = functor(x);
            return kde;
        };

        return kde;
    }

    function extent(arr) {
        return arr.reduce(function (acc, v) {
            return [Math.min(acc[0], v), Math.max(acc[1], v)];
        }, [Number.POSITIVE_INFINITY, Number.NEGATIVE_INFINITY]);
    }

    function whiskers(boxplot, arr) {
        var iqr = boxplot.q3 - boxplot.q1; // since top left is max

        var whiskerMin = Math.max(boxplot.min, boxplot.q1 - iqr);
        var whiskerMax = Math.min(boxplot.max, boxplot.q3 + iqr);

        if (Array.isArray(arr)) {
            // compute the closest real element
            for (var i = 0; i < arr.length; i++) {
                var v = arr[i];

                if (v >= whiskerMin) {
                    whiskerMin = v;
                    break;
                }
            }

            for (var _i = arr.length - 1; _i >= 0; _i--) {
                var _v = arr[_i];

                if (_v <= whiskerMax) {
                    whiskerMax = _v;
                    break;
                }
            }
        }

        return {
            whiskerMin: whiskerMin,
            whiskerMax: whiskerMax
        };
    }
    function boxplotStats(arr) {
        // console.assert(Array.isArray(arr));
        if (arr.length === 0) {
            return {
                min: NaN,
                max: NaN,
                median: NaN,
                q1: NaN,
                q3: NaN,
                whiskerMin: NaN,
                whiskerMax: NaN,
                outliers: []
            };
        }

        arr = arr.filter(function (v) {
            return typeof v === 'number' && !isNaN(v);
        });
        arr.sort(function (a, b) {
            return a - b;
        });

        var _quantiles = quantiles(arr, [0.5, 0.25, 0.75]),
            _quantiles2 = _slicedToArray(_quantiles, 3),
            median = _quantiles2[0],
            q1 = _quantiles2[1],
            q3 = _quantiles2[2];

        var minmax = extent(arr);
        var base = {
            min: minmax[0],
            max: minmax[1],
            median: median,
            q1: q1,
            q3: q3,
            outliers: []
        };

        var _whiskers = whiskers(base, arr),
            whiskerMin = _whiskers.whiskerMin,
            whiskerMax = _whiskers.whiskerMax;

        base.outliers = arr.filter(function (v) {
            return v < whiskerMin || v > whiskerMax;
        });
        base.whiskerMin = whiskerMin;
        base.whiskerMax = whiskerMax;
        return base;
    }
    function violinStats(arr) {
        // console.assert(Array.isArray(arr));
        if (arr.length === 0) {
            return {
                outliers: []
            };
        }

        arr = arr.filter(function (v) {
            return typeof v === 'number' && !isNaN(v);
        });
        arr.sort(function (a, b) {
            return a - b;
        });
        var minmax = extent(arr);
        return {
            min: minmax[0],
            max: minmax[1],
            median: quantiles(arr, [0.5])[0],
            kde: kde().sample(arr)
        };
    }
    function asBoxPlotStats(value) {
        if (!value) {
            return null;
        }

        if (typeof value.median === 'number' && typeof value.q1 === 'number' && typeof value.q3 === 'number') {
            // sounds good, check for helper
            if (typeof value.whiskerMin === 'undefined') {
                var _whiskers2 = whiskers(value, Array.isArray(value.items) ? value.items.slice().sort(function (a, b) {
                    return a - b;
                }) : null),
                    whiskerMin = _whiskers2.whiskerMin,
                    whiskerMax = _whiskers2.whiskerMax;

                value.whiskerMin = whiskerMin;
                value.whiskerMax = whiskerMax;
            }

            return value;
        }

        if (!Array.isArray(value)) {
            return undefined;
        }

        if (value.__stats === undefined) {
            value.__stats = boxplotStats(value);
        }

        return value.__stats;
    }
    function asViolinStats(value) {
        if (!value) {
            return null;
        }

        if (typeof value.median === 'number' && (typeof value.kde === 'function' || Array.isArray(value.coords))) {
            return value;
        }

        if (!Array.isArray(value)) {
            return undefined;
        }

        if (value.__kde === undefined) {
            value.__kde = violinStats(value);
        }

        return value.__kde;
    }
    function asValueStats(value, minStats, maxStats) {
        if (typeof value[minStats] === 'number' && typeof value[maxStats] === 'number') {
            return value;
        }

        if (!Array.isArray(value) || value.length === 0) {
            return undefined;
        }

        return asBoxPlotStats(value);
    }
    function getRightValue(rawValue) {
        if (!rawValue) {
            return rawValue;
        }

        if (typeof rawValue === 'number' || typeof rawValue === 'string') {
            return Number(rawValue);
        }

        var b = asBoxPlotStats(rawValue);
        return b ? b.median : rawValue;
    }
    var commonScaleOptions = {
        ticks: {
            minStats: 'min',
            maxStats: 'max'
        }
    };
    function commonDataLimits(extraCallback) {
        var _this = this;

        var chart = this.chart;
        var isHorizontal = this.isHorizontal();
        var tickOpts = this.options.ticks;
        var minStats = tickOpts.minStats;
        var maxStats = tickOpts.maxStats;

        var matchID = function matchID(meta) {
            return isHorizontal ? meta.xAxisID === _this.id : meta.yAxisID === _this.id;
        }; // First Calculate the range


        this.min = null;
        this.max = null; // Regular charts use x, y values
        // For the boxplot chart we have rawValue.min and rawValue.max for each point

        chart.data.datasets.forEach(function (d, i) {
            var meta = chart.getDatasetMeta(i);

            if (!chart.isDatasetVisible(i) || !matchID(meta)) {
                return;
            }

            d.data.forEach(function (value, j) {
                if (!value || meta.data[j].hidden) {
                    return;
                }

                var stats = asValueStats(value, minStats, maxStats);

                if (!stats) {
                    return;
                }

                if (_this.min === null || stats[minStats] < _this.min) {
                    _this.min = stats[minStats];
                }

                if (_this.max === null || stats[maxStats] > _this.max) {
                    _this.max = stats[maxStats];
                }

                if (extraCallback) {
                    extraCallback(stats);
                }
            });
        });
    }
    function rnd(seed) {
        // Adapted from http://indiegamr.com/generate-repeatable-random-numbers-in-js/
        if (seed === undefined) {
            seed = Date.now();
        }

        return function () {
            seed = (seed * 9301 + 49297) % 233280;
            return seed / 233280;
        };
    }

    var defaults = _objectSpread({}, Chart.defaults.global.elements.rectangle, {
        borderWidth: 1,
        outlierRadius: 2,
        outlierColor: Chart.defaults.global.elements.rectangle.backgroundColor,
        medianColor: null,
        itemRadius: 0,
        itemStyle: 'circle',
        itemBackgroundColor: Chart.defaults.global.elements.rectangle.backgroundColor,
        itemBorderColor: Chart.defaults.global.elements.rectangle.borderColor,
        hitPadding: 2,
        tooltipDecimals: 2
    });
    var ArrayElementBase = Chart.Element.extend({
        isVertical: function isVertical() {
            return this._view.width !== undefined;
        },
        draw: function draw() {// abstract
        },
        _drawItems: function _drawItems(vm, container, ctx, vert) {
            if (vm.itemRadius <= 0 || !container.items || container.items.length <= 0) {
                return;
            }

            ctx.save();
            ctx.strokeStle = vm.itemBorderColor;
            ctx.fillStyle = vm.itemBackgroundColor; // jitter based on random data
            // use the datesetindex and index to initialize the random number generator

            var random = rnd(this._datasetIndex * 1000 + this._index);

            if (vert) {
                container.items.forEach(function (v) {
                    Chart.canvasHelpers.drawPoint(ctx, vm.itemStyle, vm.itemRadius, vm.x - vm.width / 2 + random() * vm.width, v);
                });
            } else {
                container.items.forEach(function (v) {
                    Chart.canvasHelpers.drawPoint(ctx, vm.itemStyle, vm.itemRadius, v, vm.y - vm.height / 2 + random() * vm.height);
                });
            }

            ctx.restore();
        },
        _drawOutliers: function _drawOutliers(vm, container, ctx, vert) {
            if (!container.outliers) {
                return;
            }

            ctx.fillStyle = vm.outlierColor;
            ctx.beginPath();

            if (vert) {
                container.outliers.forEach(function (v) {
                    ctx.arc(vm.x, v, vm.outlierRadius, 0, Math.PI * 2);
                });
            } else {
                container.outliers.forEach(function (v) {
                    ctx.arc(v, vm.y, vm.outlierRadius, 0, Math.PI * 2);
                });
            }

            ctx.fill();
            ctx.closePath();
        },
        _getBounds: function _getBounds() {
            // abstract
            return {
                left: 0,
                top: 0,
                right: 0,
                bottom: 0
            };
        },
        _getHitBounds: function _getHitBounds() {
            var padding = this._view.hitPadding;

            var b = this._getBounds();

            return {
                left: b.left - padding,
                top: b.top - padding,
                right: b.right + padding,
                bottom: b.bottom + padding
            };
        },
        height: function height() {
            return 0; // abstract
        },
        inRange: function inRange(mouseX, mouseY) {
            if (!this._view) {
                return false;
            }

            var bounds = this._getHitBounds();

            return mouseX >= bounds.left && mouseX <= bounds.right && mouseY >= bounds.top && mouseY <= bounds.bottom;
        },
        inLabelRange: function inLabelRange(mouseX, mouseY) {
            if (!this._view) {
                return false;
            }

            var bounds = this._getHitBounds();

            if (this.isVertical()) {
                return mouseX >= bounds.left && mouseX <= bounds.right;
            }

            return mouseY >= bounds.top && mouseY <= bounds.bottom;
        },
        inXRange: function inXRange(mouseX) {
            var bounds = this._getHitBounds();

            return mouseX >= bounds.left && mouseX <= bounds.right;
        },
        inYRange: function inYRange(mouseY) {
            var bounds = this._getHitBounds();

            return mouseY >= bounds.top && mouseY <= bounds.bottom;
        },
        getCenterPoint: function getCenterPoint() {
            var _this$_view = this._view,
                x = _this$_view.x,
                y = _this$_view.y;
            return {
                x: x,
                y: y
            };
        },
        getArea: function getArea() {
            return 0; // abstract
        },
        tooltipPosition_: function tooltipPosition_() {
            return this.getCenterPoint();
        }
    });

    Chart.defaults.global.elements.boxandwhiskers = _objectSpread({}, defaults);

    function transitionBoxPlot(start, view, model, ease) {
        var keys = Object.keys(model);

        for (var _i = 0; _i < keys.length; _i++) {
            var key = keys[_i];
            var target = model[key];
            var origin = start[key];

            if (origin === target) {
                continue;
            }

            if (typeof target === 'number') {
                view[key] = origin + (target - origin) * ease;
                continue;
            }

            if (Array.isArray(target)) {
                var v = view[key];
                var common = Math.min(target.length, origin.length);

                for (var i = 0; i < common; ++i) {
                    v[i] = origin[i] + (target[i] - origin[i]) * ease;
                }
            }
        }
    }

    var BoxAndWiskers = Chart.elements.BoxAndWhiskers = ArrayElementBase.extend({
        transition: function transition(ease) {
            var r = Chart.Element.prototype.transition.call(this, ease);
            var model = this._model;
            var start = this._start;
            var view = this._view; // No animation -> No Transition

            if (!model || ease === 1) {
                return r;
            }

            if (start.boxplot == null) {
                return r; // model === view -> not copied
            } // create deep copy to avoid alternation


            if (model.boxplot === view.boxplot) {
                view.boxplot = Chart.helpers.clone(view.boxplot);
            }

            transitionBoxPlot(start.boxplot, view.boxplot, model.boxplot, ease);
            return r;
        },
        draw: function draw() {
            var ctx = this._chart.ctx;
            var vm = this._view;
            var boxplot = vm.boxplot;
            var vert = this.isVertical();

            this._drawItems(vm, boxplot, ctx, vert);

            ctx.save();
            ctx.fillStyle = vm.backgroundColor;
            ctx.strokeStyle = vm.borderColor;
            ctx.lineWidth = vm.borderWidth;

            this._drawBoxPlot(vm, boxplot, ctx, vert);

            this._drawOutliers(vm, boxplot, ctx, vert);

            ctx.restore();
        },
        _drawBoxPlot: function _drawBoxPlot(vm, boxplot, ctx, vert) {
            if (vert) {
                this._drawBoxPlotVert(vm, boxplot, ctx);
            } else {
                this._drawBoxPlotHoriz(vm, boxplot, ctx);
            }
        },
        _drawBoxPlotVert: function _drawBoxPlotVert(vm, boxplot, ctx) {
            var x = vm.x;
            var width = vm.width;
            var x0 = x - width / 2; // Draw the q1>q3 box

            if (boxplot.q3 > boxplot.q1) {
                ctx.fillRect(x0, boxplot.q1, width, boxplot.q3 - boxplot.q1);
            } else {
                ctx.fillRect(x0, boxplot.q3, width, boxplot.q1 - boxplot.q3);
            } // Draw the median line


            ctx.save();

            if (vm.medianColor) {
                ctx.strokeStyle = vm.medianColor;
            }

            ctx.beginPath();
            ctx.moveTo(x0, boxplot.median);
            ctx.lineTo(x0 + width, boxplot.median);
            ctx.closePath();
            ctx.stroke();
            ctx.restore(); // Draw the border around the main q1>q3 box

            if (boxplot.q3 > boxplot.q1) {
                ctx.strokeRect(x0, boxplot.q1, width, boxplot.q3 - boxplot.q1);
            } else {
                ctx.strokeRect(x0, boxplot.q3, width, boxplot.q1 - boxplot.q3);
            } // Draw the whiskers


            ctx.beginPath();
            ctx.moveTo(x0, boxplot.whiskerMin);
            ctx.lineTo(x0 + width, boxplot.whiskerMin);
            ctx.moveTo(x, boxplot.whiskerMin);
            ctx.lineTo(x, boxplot.q1);
            ctx.moveTo(x0, boxplot.whiskerMax);
            ctx.lineTo(x0 + width, boxplot.whiskerMax);
            ctx.moveTo(x, boxplot.whiskerMax);
            ctx.lineTo(x, boxplot.q3);
            ctx.closePath();
            ctx.stroke();
        },
        _drawBoxPlotHoriz: function _drawBoxPlotHoriz(vm, boxplot, ctx) {
            var y = vm.y;
            var height = vm.height;
            var y0 = y - height / 2; // Draw the q1>q3 box

            if (boxplot.q3 > boxplot.q1) {
                ctx.fillRect(boxplot.q1, y0, boxplot.q3 - boxplot.q1, height);
            } else {
                ctx.fillRect(boxplot.q3, y0, boxplot.q1 - boxplot.q3, height);
            } // Draw the median line


            ctx.save();

            if (vm.medianColor) {
                ctx.strokeStyle = vm.medianColor;
            }

            ctx.beginPath();
            ctx.moveTo(boxplot.median, y0);
            ctx.lineTo(boxplot.median, y0 + height);
            ctx.closePath();
            ctx.stroke();
            ctx.restore(); // Draw the border around the main q1>q3 box

            if (boxplot.q3 > boxplot.q1) {
                ctx.strokeRect(boxplot.q1, y0, boxplot.q3 - boxplot.q1, height);
            } else {
                ctx.strokeRect(boxplot.q3, y0, boxplot.q1 - boxplot.q3, height);
            } // Draw the whiskers


            ctx.beginPath();
            ctx.moveTo(boxplot.whiskerMin, y0);
            ctx.lineTo(boxplot.whiskerMin, y0 + height);
            ctx.moveTo(boxplot.whiskerMin, y);
            ctx.lineTo(boxplot.q1, y);
            ctx.moveTo(boxplot.whiskerMax, y0);
            ctx.lineTo(boxplot.whiskerMax, y0 + height);
            ctx.moveTo(boxplot.whiskerMax, y);
            ctx.lineTo(boxplot.q3, y);
            ctx.closePath();
            ctx.stroke();
        },
        _getBounds: function _getBounds() {
            var vm = this._view;
            var vert = this.isVertical();
            var boxplot = vm.boxplot;

            if (!boxplot) {
                return {
                    left: 0,
                    top: 0,
                    right: 0,
                    bottom: 0
                };
            }

            if (vert) {
                var x = vm.x,
                    width = vm.width;
                var x0 = x - width / 2;
                return {
                    left: x0,
                    top: boxplot.whiskerMax,
                    right: x0 + width,
                    bottom: boxplot.whiskerMin
                };
            }

            var y = vm.y,
                height = vm.height;
            var y0 = y - height / 2;
            return {
                left: boxplot.whiskerMin,
                top: y0,
                right: boxplot.whiskerMax,
                bottom: y0 + height
            };
        },
        height: function height() {
            var vm = this._view;
            return vm.base - Math.min(vm.boxplot.q1, vm.boxplot.q3);
        },
        getArea: function getArea() {
            var vm = this._view;
            var iqr = Math.abs(vm.boxplot.q3 - vm.boxplot.q1);

            if (this.isVertical()) {
                return iqr * vm.width;
            }

            return iqr * vm.height;
        }
    });

    Chart.defaults.global.elements.violin = _objectSpread({
        points: 100
    }, defaults);

    function transitionViolin(start, view, model, ease) {
        var keys = Object.keys(model);

        for (var _i = 0; _i < keys.length; _i++) {
            var key = keys[_i];
            var target = model[key];
            var origin = start[key];

            if (origin === target) {
                continue;
            }

            if (typeof target === 'number') {
                view[key] = origin + (target - origin) * ease;
                continue;
            }

            if (key === 'coords') {
                var v = view[key];
                var common = Math.min(target.length, origin.length);

                for (var i = 0; i < common; ++i) {
                    v[i].v = origin[i].v + (target[i].v - origin[i].v) * ease;
                    v[i].estimate = origin[i].estimate + (target[i].estimate - origin[i].estimate) * ease;
                }
            }
        }
    }

    var Violin = Chart.elements.Violin = ArrayElementBase.extend({
        transition: function transition(ease) {
            var r = Chart.Element.prototype.transition.call(this, ease);
            var model = this._model;
            var start = this._start;
            var view = this._view; // No animation -> No Transition

            if (!model || ease === 1) {
                return r;
            }

            if (start.violin == null) {
                return r; // model === view -> not copied
            } // create deep copy to avoid alternation


            if (model.violin === view.violin) {
                view.violin = Chart.helpers.clone(view.violin);
            }

            transitionViolin(start.violin, view.violin, model.violin, ease);
            return r;
        },
        draw: function draw() {
            var ctx = this._chart.ctx;
            var vm = this._view;
            var violin = vm.violin;
            var vert = this.isVertical();

            this._drawItems(vm, violin, ctx, vert);

            ctx.save();
            ctx.fillStyle = vm.backgroundColor;
            ctx.strokeStyle = vm.borderColor;
            ctx.lineWidth = vm.borderWidth;
            var coords = violin.coords;
            Chart.canvasHelpers.drawPoint(ctx, 'rectRot', 5, vm.x, vm.y);
            ctx.stroke();
            ctx.beginPath();

            if (vert) {
                var x = vm.x;
                var width = vm.width;
                var factor = width / 2 / violin.maxEstimate;
                ctx.moveTo(x, violin.min);
                coords.forEach(function (_ref) {
                    var v = _ref.v,
                        estimate = _ref.estimate;
                    ctx.lineTo(x - estimate * factor, v);
                });
                ctx.lineTo(x, violin.max);
                ctx.moveTo(x, violin.min);
                coords.forEach(function (_ref2) {
                    var v = _ref2.v,
                        estimate = _ref2.estimate;
                    ctx.lineTo(x + estimate * factor, v);
                });
                ctx.lineTo(x, violin.max);
            } else {
                var y = vm.y;
                var height = vm.height;

                var _factor = height / 2 / violin.maxEstimate;

                ctx.moveTo(violin.min, y);
                coords.forEach(function (_ref3) {
                    var v = _ref3.v,
                        estimate = _ref3.estimate;
                    ctx.lineTo(v, y - estimate * _factor);
                });
                ctx.lineTo(violin.max, y);
                ctx.moveTo(violin.min, y);
                coords.forEach(function (_ref4) {
                    var v = _ref4.v,
                        estimate = _ref4.estimate;
                    ctx.lineTo(v, y + estimate * _factor);
                });
                ctx.lineTo(violin.max, y);
            }

            ctx.stroke();
            ctx.fill();
            ctx.closePath();

            this._drawOutliers(vm, violin, ctx, vert);

            ctx.restore();
        },
        _getBounds: function _getBounds() {
            var vm = this._view;
            var vert = this.isVertical();
            var violin = vm.violin;

            if (vert) {
                var x = vm.x,
                    width = vm.width;
                var x0 = x - width / 2;
                return {
                    left: x0,
                    top: violin.max,
                    right: x0 + width,
                    bottom: violin.min
                };
            }

            var y = vm.y,
                height = vm.height;
            var y0 = y - height / 2;
            return {
                left: violin.min,
                top: y0,
                right: violin.max,
                bottom: y0 + height
            };
        },
        height: function height() {
            var vm = this._view;
            return vm.base - Math.min(vm.violin.min, vm.violin.max);
        },
        getArea: function getArea() {
            var vm = this._view;
            var iqr = Math.abs(vm.violin.max - vm.violin.min);

            if (this.isVertical()) {
                return iqr * vm.width;
            }

            return iqr * vm.height;
        }
    });

    var verticalDefaults = {
        scales: {
            yAxes: [{
                type: 'arrayLinear'
            }]
        }
    };
    var horizontalDefaults = {
        scales: {
            xAxes: [{
                type: 'arrayLinear'
            }]
        }
    };
    function toFixed(value) {
        var decimals = this._chart.config.options.tooltipDecimals; // inject number of decimals from config

        if (decimals == null || typeof decimals !== 'number' || decimals < 0) {
            return value;
        }

        return Number.parseFloat(value).toFixed(decimals);
    }
    var array = {
        _elementOptions: function _elementOptions() {
            return {};
        },
        updateElement: function updateElement(elem, index, reset) {
            var dataset = this.getDataset();
            var custom = elem.custom || {};

            var options = this._elementOptions();

            Chart.controllers.bar.prototype.updateElement.call(this, elem, index, reset);
            var resolve = Chart.helpers.options.resolve;
            var keys = ['outlierRadius', 'itemRadius', 'itemStyle', 'itemBackgroundColor', 'itemBorderColor', 'outlierColor', 'medianColor', 'hitPadding']; // Scriptable options

            var context = {
                chart: this.chart,
                dataIndex: index,
                dataset: dataset,
                datasetIndex: this.index
            };
            keys.forEach(function (item) {
                elem._model[item] = resolve([custom[item], dataset[item], options[item]], context, index);
            });
        },
        _calculateCommonModel: function _calculateCommonModel(r, data, container, scale) {
            if (container.outliers) {
                r.outliers = container.outliers.map(function (d) {
                    return scale.getPixelForValue(Number(d));
                });
            }

            if (Array.isArray(data)) {
                r.items = data.map(function (d) {
                    return scale.getPixelForValue(Number(d));
                });
            }
        }
    };

    var defaults$1 = {
        tooltips: {
            callbacks: {
                label: function label(item, data) {
                    var datasetLabel = data.datasets[item.datasetIndex].label || '';
                    var value = data.datasets[item.datasetIndex].data[item.index];
                    var b = asBoxPlotStats(value);
                    var label = "".concat(datasetLabel, " ").concat(typeof item.xLabel === 'string' ? item.xLabel : item.yLabel);

                    if (!b) {
                        return "".concat(label, " (NaN)");
                    }

                    return "".concat(label, " (min: ").concat(toFixed.call(this, b.min), ", q1: ").concat(toFixed.call(this, b.q1), ", median: ").concat(toFixed.call(this, b.median), ", q3: ").concat(toFixed.call(this, b.q3), ", max: ").concat(toFixed.call(this, b.max), ")");
                }
            }
        }
    };
    Chart.defaults.boxplot = Chart.helpers.merge({}, [Chart.defaults.bar, verticalDefaults, defaults$1]);
    Chart.defaults.horizontalBoxplot = Chart.helpers.merge({}, [Chart.defaults.horizontalBar, horizontalDefaults, defaults$1]);

    var boxplot = _objectSpread({}, array, {
        dataElementType: Chart.elements.BoxAndWhiskers,
        _elementOptions: function _elementOptions() {
            return this.chart.options.elements.boxandwhiskers;
        },

        /**
         * @private
         */
        _updateElementGeometry: function _updateElementGeometry(elem, index, reset) {
            Chart.controllers.bar.prototype._updateElementGeometry.call(this, elem, index, reset);

            elem._model.boxplot = this._calculateBoxPlotValuesPixels(this.index, index);
        },

        /**
         * @private
         */
        _calculateBoxPlotValuesPixels: function _calculateBoxPlotValuesPixels(datasetIndex, index) {
            var scale = this._getValueScale();

            var data = this.chart.data.datasets[datasetIndex].data[index];

            if (!data) {
                return null;
            }

            var v = asBoxPlotStats(data);
            var r = {};
            Object.keys(v).forEach(function (key) {
                if (key !== 'outliers') {
                    r[key] = scale.getPixelForValue(Number(v[key]));
                }
            });

            this._calculateCommonModel(r, data, v, scale);

            return r;
        }
    });
    /**
     * This class is based off controller.bar.js from the upstream Chart.js library
     */


    var BoxPlot = Chart.controllers.boxplot = Chart.controllers.bar.extend(boxplot);
    var HorizontalBoxPlot = Chart.controllers.horizontalBoxplot = Chart.controllers.horizontalBar.extend(boxplot);

    var defaults$2 = {
        tooltips: {
            callbacks: {
                label: function label(item, data) {
                    var datasetLabel = data.datasets[item.datasetIndex].label || '';
                    var value = item.value;
                    var label = "".concat(datasetLabel, " ").concat(typeof item.xLabel === 'string' ? item.xLabel : item.yLabel);
                    return "".concat(label, " (").concat(toFixed.call(this, value), ")");
                }
            }
        }
    };
    Chart.defaults.violin = Chart.helpers.merge({}, [Chart.defaults.bar, verticalDefaults, defaults$2]);
    Chart.defaults.horizontalViolin = Chart.helpers.merge({}, [Chart.defaults.horizontalBar, horizontalDefaults, defaults$2]);

    var controller = _objectSpread({}, array, {
        dataElementType: Chart.elements.Violin,
        _elementOptions: function _elementOptions() {
            return this.chart.options.elements.violin;
        },

        /**
         * @private
         */
        _updateElementGeometry: function _updateElementGeometry(elem, index, reset) {
            Chart.controllers.bar.prototype._updateElementGeometry.call(this, elem, index, reset);

            var custom = elem.custom || {};

            var options = this._elementOptions();

            elem._model.violin = this._calculateViolinValuesPixels(this.index, index, custom.points !== undefined ? custom.points : options.points);
        },

        /**
         * @private
         */
        _calculateViolinValuesPixels: function _calculateViolinValuesPixels(datasetIndex, index, points) {
            var scale = this._getValueScale();

            var data = this.chart.data.datasets[datasetIndex].data[index];
            var violin = asViolinStats(data);

            if (!Array.isArray(data) && typeof data === 'number' && !Number.isNaN || violin == null) {
                return {
                    min: data,
                    max: data,
                    median: data,
                    coords: [{
                        v: data,
                        estimate: Number.NEGATIVE_INFINITY
                    }],
                    maxEstimate: Number.NEGATIVE_INFINITY
                };
            }

            var range = violin.max - violin.min;
            var samples = [];
            var inc = range / points;

            for (var v = violin.min; v <= violin.max && inc > 0; v += inc) {
                samples.push(v);
            }

            if (samples[samples.length - 1] !== violin.max) {
                samples.push(violin.max);
            }

            var coords = violin.coords || violin.kde(samples).map(function (v) {
                return {
                    v: v[0],
                    estimate: v[1]
                };
            });
            var r = {
                min: scale.getPixelForValue(violin.min),
                max: scale.getPixelForValue(violin.max),
                median: scale.getPixelForValue(violin.median),
                coords: coords.map(function (_ref) {
                    var v = _ref.v,
                        estimate = _ref.estimate;
                    return {
                        v: scale.getPixelForValue(v),
                        estimate: estimate
                    };
                }),
                maxEstimate: coords.reduce(function (a, d) {
                    return Math.max(a, d.estimate);
                }, Number.NEGATIVE_INFINITY)
            };

            this._calculateCommonModel(r, data, violin, scale);

            return r;
        }
    });
    /**
     * This class is based off controller.bar.js from the upstream Chart.js library
     */


    var Violin$1 = Chart.controllers.violin = Chart.controllers.bar.extend(controller);
    var HorizontalViolin = Chart.controllers.horizontalViolin = Chart.controllers.horizontalBar.extend(controller);

    var helpers = Chart.helpers;
    var ArrayLinearScaleOptions = helpers.merge({}, [commonScaleOptions, Chart.scaleService.getScaleDefaults('linear')]);
    var ArrayLinearScale = Chart.scaleService.getScaleConstructor('linear').extend({
        getRightValue: function getRightValue$$1(rawValue) {
            return Chart.LinearScaleBase.prototype.getRightValue.call(this, getRightValue(rawValue));
        },
        determineDataLimits: function determineDataLimits() {
            commonDataLimits.call(this); // Common base implementation to handle ticks.min, ticks.max, ticks.beginAtZero

            this.handleTickRangeOptions();
        }
    });
    Chart.scaleService.registerScaleType('arrayLinear', ArrayLinearScale, ArrayLinearScaleOptions);

    var helpers$1 = Chart.helpers;
    var ArrayLogarithmicScaleOptions = helpers$1.merge({}, [commonScaleOptions, Chart.scaleService.getScaleDefaults('logarithmic')]);
    var ArrayLogarithmicScale = Chart.scaleService.getScaleConstructor('logarithmic').extend({
        getRightValue: function getRightValue$$1(rawValue) {
            return Chart.LinearScaleBase.prototype.getRightValue.call(this, getRightValue(rawValue));
        },
        determineDataLimits: function determineDataLimits() {
            var _this = this;

            // Add whitespace around bars. Axis shouldn't go exactly from min to max
            var tickOpts = this.options.ticks;
            this.minNotZero = null;
            commonDataLimits.call(this, function (boxPlot) {
                var value = boxPlot[tickOpts.minStats];

                if (value !== 0 && (_this.minNotZero === null || value < _this.minNotZero)) {
                    _this.minNotZero = value;
                }
            });
            this.min = helpers$1.valueOrDefault(tickOpts.min, this.min - this.min * 0.05);
            this.max = helpers$1.valueOrDefault(tickOpts.max, this.max + this.max * 0.05);

            if (this.min === this.max) {
                if (this.min !== 0 && this.min !== null) {
                    this.min = Math.pow(10, Math.floor(helpers$1.log10(this.min)) - 1);
                    this.max = Math.pow(10, Math.floor(helpers$1.log10(this.max)) + 1);
                } else {
                    this.min = 1;
                    this.max = 10;
                }
            }
        }
    });
    Chart.scaleService.registerScaleType('arrayLogarithmic', ArrayLogarithmicScale, ArrayLogarithmicScaleOptions);

    exports.BoxAndWhiskers = BoxAndWiskers;
    exports.Violin = Violin;
    exports.ArrayLinearScale = ArrayLinearScale;
    exports.ArrayLogarithmicScale = ArrayLogarithmicScale;
    exports.BoxPlot = BoxPlot;
    exports.HorizontalBoxPlot = HorizontalBoxPlot;
    exports.HorizontalViolin = HorizontalViolin;

    Object.defineProperty(exports, '__esModule', { value: true });

}));
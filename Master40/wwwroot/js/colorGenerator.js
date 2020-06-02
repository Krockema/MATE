const golden_ratio = 0.618033988749895;
function getRandomInt(max)
{
    return Math.floor(Math.random() * Math.floor(max));
}
function ColorAbc()
{
    var htmlOut = " ";
    var randomNr = Math.random();

    for (i = 0; i < 27; i++) {
        var hsv = goldenRatioColor(i, randomNr, 0.95, 0.95);

        htmlOut = htmlOut +
            "<span style=\"background-color: " + ColorArrayToRgba(hsv, 1) + "; padding:5px; -moz-border-radius:3px; -webkit-border-radius:3px;\">" + i + "</span>";
    }
    return htmlOut;
}

function hsvToRgb(h , s, v) {
    var r = 0, g = 0, b = 0;

    h_i = Math.floor(h * 6);
    f = h * 6 - h_i;
    p = v * (1 - s);
    q = v * (1 - f * s);
    t = v * (1 - (1 - f) * s);

    if (h_i == 0) { r = v; g = t; b = p; }
    if (h_i == 1) { r = q; g = v; b = p; }
    if (h_i == 2) { r = p; g = v; b = t; }
    if (h_i == 3) { r = p; g = q; b = v; }
    if (h_i == 4) { r = t; g = p; b = v; }
    if (h_i == 5) { r = v; g = p; b = q; }
    return [Math.floor(r * 255), Math.floor(g * 255), Math.floor(b * 255)];
}

function goldenRatioColor(index, hue, saturation, value) {
    var hue = (hue + (golden_ratio * index)) % 1;
    return hsvToRgb(hue, saturation, value);
}

function ColorArrayToRgba(colorArray, alpha) {
    return "rgba(" + colorArray[0] + ", " + colorArray[1] + "," + colorArray[2] + " ," + alpha + ")";
}
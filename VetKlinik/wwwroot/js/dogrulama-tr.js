// jQuery Validation'a Türkçe sayı biçimini öğretir (Hafta 6).
// Kütüphane "120.50" bekler; sunucu tr-TR'de "120,50" bekler (Program.cs).
// Kabul edilen biçim: 120,50 · 1.250,50 · 99   (nokta binlik, virgül ondalık)
(function ($) {
    if (!$ || !$.validator) return;

    var TR = /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/;

    function sayiya(metin) {
        return parseFloat(metin.replace(/\./g, '').replace(',', '.'));
    }

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || TR.test(value);
    };

    $.validator.methods.range = function (value, element, param) {
        if (this.optional(element)) return true;
        var n = TR.test(value) ? sayiya(value) : NaN;
        return n >= Number(param[0]) && n <= Number(param[1]);
    };
})(window.jQuery);

// Randevu Al sayfası: hizmet veya tarih değişince REST API'den uygun saatleri çeker.
// GET /api/veterinerler/uygun?hizmetId=1&tarih=2026-11-23
// Seçilen düğme gizli "Secim" alanına "hekimId|SS:dd" olarak yazılır.
(function ($) {
  // DOM hazır olunca: unobtrusive doğrulama kendi kurulumunu bitirmiş olur
  $(function () {
    var $form = $('#randevuFormu');
    if ($form.length === 0) return;

    var $hizmet = $('#HizmetId');
    var $tarih = $('#Tarih');
    var $kutu = $('#uygunSaatler');
    var $secim = $('#Secim');

    // jQuery Validation varsayılan olarak gizli alanları atlar; Secim'i doğrulasın
    var dogrulayici = $form.data('validator');
    if (dogrulayici) dogrulayici.settings.ignore = [];

    function mesaj(metin, sinif) {
        $kutu.empty().append($('<span>').addClass(sinif || 'text-muted').text(metin));
    }

    function yukle() {
        var hizmetId = $hizmet.val();
        var tarih = $tarih.val();
        if (!hizmetId || !tarih) {
            mesaj('Hizmet ve tarih seçin.');
            return;
        }

        mesaj('Uygun saatler yükleniyor…');

        $.getJSON('/api/veterinerler/uygun', { hizmetId: hizmetId, tarih: tarih })
            .done(function (veri) {
                $kutu.empty();
                if (!veri.hekimler || veri.hekimler.length === 0) {
                    mesaj('Bu gün için boş saat yok. Başka bir gün deneyin.', 'text-danger');
                    $secim.val('');
                    return;
                }

                var onceki = $secim.val() || $kutu.data('secili') || '';
                var bulundu = false;

                veri.hekimler.forEach(function (h) {
                    var $blok = $('<div class="mb-3">');
                    $blok.append($('<div class="fw-semibold mb-1">').text(h.adSoyad + ' — ' + h.uzmanAlani));
                    var $grup = $('<div class="slot-grup">');

                    h.saatler.forEach(function (saat) {
                        var deger = h.veterinerId + '|' + saat;
                        var id = 'slot-' + h.veterinerId + '-' + saat.replace(':', '');
                        var $radyo = $('<input type="radio" class="btn-check" name="slotSecici" autocomplete="off">')
                            .attr('id', id).val(deger);
                        if (deger === onceki) { $radyo.prop('checked', true); bulundu = true; }
                        $grup.append($radyo, $('<label class="btn btn-outline-primary btn-sm">').attr('for', id).text(saat));
                    });

                    $blok.append($grup);
                    $kutu.append($blok);
                });

                $kutu.append($('<div class="small text-muted">').text(veri.hizmetAdi + ' ' + veri.sureDakika + ' dakika sürer.'));
                $secim.val(bulundu ? onceki : '');
            })
            .fail(function (xhr) {
                var hata = xhr.responseJSON && xhr.responseJSON.hata ? xhr.responseJSON.hata : 'Saatler alınamadı.';
                mesaj(hata, 'text-danger');
                $secim.val('');
            });
    }

    $kutu.on('change', 'input[name="slotSecici"]', function () {
        $secim.val($(this).val());
        if (dogrulayici) $form.validate().element($secim);
    });

    $hizmet.on('change', yukle);
    $tarih.on('change', yukle);
    yukle();   // sayfa açılırken (ya da hatalı form geri geldiğinde) seçimi yeniden kur
  });
})(window.jQuery);

# YouthRise — revisi permintaan dosen

## Yang bisa didemokan sekarang

1. **Audio:** musik instrumental ambient orisinal yang dibuat saat runtime, looping 24 detik. Tombol Musik dan Narasi pada menu/HUD; Ulang Suara pada cerita. Musik mengecil saat narasi aktif. Musik default ON, narasi default OFF; preferensi tersimpan pada perangkat.
2. **Tombol:** rounded rectangle untuk menu, pilihan cerita, Safe Zone dan layar baru. Tidak menambah aset bitmap untuk bentuk tombol.
3. **Meter:** tersembunyi sepanjang cerita, termasuk perubahan skor pada feedback pilihan. Sesudah chapter selesai: **review Risk/Trust dan empat indikator → reflection → kelanjutan chapter/ending**. Nilai tetap kumulatif; bukan skor klinis. XP dan unlock lama tetap dipertahankan.
4. **Safe Zone:** pendamping lokal dengan respons berdasarkan topik; konektor bot AI disiapkan tetapi nonaktif. Pengaduan ditujukan ke WhatsApp Guru BK hanya jika server dikonfigurasi, pemain meninjau teks dan menyetujui pengiriman.
5. **Ringkasan delapan isu:** buka Safe Zone → Need Extra Help? → Ringkasan Pilihan 8 Isu. Preview dapat digulir dan disimpan sebagai teks lokal; pengaduan insiden tetap terpisah.

### Narasi

Narasi membacakan teks cerita yang sedang tampil, termasuk varian PCG terpilih; bukan chat atau pengaduan. Fallback Windows memakai `System.Speech` lokal tanpa mengirim teks ke layanan suara. Narasi berhenti saat pindah node/layar, dinonaktifkan, atau aplikasi kehilangan fokus.

Komputer pengujian hanya menyediakan suara sistem bahasa Inggris. Narasi sudah berjalan, tetapi **pelafalan bahasa Indonesia belum layak dianggap voice-over final**. Pasang suara `id-ID` yang dapat ditemukan oleh System.Speech atau siapkan rekaman Indonesia untuk hasil yang lebih baik. Platform non-Windows membutuhkan rekaman; tidak ada janji narasi sintetis lintas platform.

Rekaman opsional dapat ditempatkan di `Resources/YouthRise/Audio/Narration/<chapter-id>/<node-id>-<hash>.wav`. Hash adalah 12 karakter heksadesimal pertama SHA-256 UTF-8 dari teks persis yang tampil. Setiap varian teks memerlukan rekaman yang cocok; jika tidak ada, Windows memakai suara sistem. Belum ada paket rekaman narasi manusia di proyek.

### Cara membaca laporan

Delapan baris mewakili tekanan teman/zat berisiko, bullying, hubungan/keamanan pribadi, kesejahteraan emosional, keuangan, gaya hidup, keseimbangan digital, serta keluarga.

Setiap baris memperlihatkan jumlah keputusan terekam dan jumlah pilihan **dukungan**, **tekanan**, serta **campuran/netral**. Ini rubrik deskriptif dari efek pilihan yang ditulis pada cerita: penurunan Risk, Anxiety, FOMO dan Social Media Dependency serta kenaikan indikator pendukung dihitung protektif; arah sebaliknya dihitung tekanan. Pilihan dengan dua arah atau tanpa perubahan dimasukkan campuran/netral. Rubrik ini belum divalidasi dan bukan diagnosis, bukti kejadian nyata, prediksi perilaku, atau dasar pemberian label/hukuman siswa.

Data bersumber dari pilihan per node yang mulai direkam pada pembaruan ini, bukan ditebak dari skor akhir. Save lama tetap bisa dilanjutkan tetapi bagian tanpa rekaman ditandai **belum ada data pilihan**. Resume tidak menghitung pilihan dua kali. Replay mengganti riwayat chapter yang diulang; New Game menghapus riwayat kampanye. Percabangan dapat melewati beberapa keputusan, sehingga cakupan tidak selalu sama dengan seluruh node.

### Data dan koneksi

Semua layanan eksternal bawaan **OFF**. Kode API tidak ditaruh di Unity; konfigurasi, persetujuan, batas demo, WhatsApp Cloud API, dan aktivasi tersedia di [panduan konektor](../../Backend/README.md).

Tambahan konektor Gemini: chat dapat memilih Gemini atau OpenAI pada server, tanpa model lokal/GPU. Gemini tidak menggantikan WhatsApp, PCG cerita, atau narasi suara. Kunci pengganti dan ID model diisi secara privat oleh pengelola; tidak ada kunci/layanan nyata yang diaktifkan dalam perubahan ini. Batas usia dan penggunaan data Gemini belum sesuai untuk deployment ke siswa 11–18; lihat ketentuan dan checklist pada panduan konektor sebelum uji live.

Di bawah `Application.persistentDataPath/YouthRise/`:

| File | Isi |
| --- | --- |
| `prototype-save.json` | Progress, indikator game, pilihan per chapter/node; tanpa chat/pengaduan |
| `Telemetry/*.jsonl` | Metadata keputusan dan snapshot indikator; tetap lokal |
| `Reports/*.json` | Draft insiden lokal yang sengaja disimpan, belum terenkripsi |
| `Reports/*.txt` | Teks preview insiden/ringkasan yang sengaja diekspor, belum dikirim |
| `Receipts/*.json` | Metadata percobaan pengiriman dan status penerimaan API bila terkonfirmasi |

Pemain meninjau isi dan penerima sebelum mengirim. Tanda `accepted_by_whatsapp` hanya berarti API menerima permintaan dengan ID pesan; tidak membuktikan Guru BK membaca atau menangani laporan. Kegagalan/timeout tidak dinyatakan sukses. Tidak ada laporan otomatis berdasarkan skor dan tidak ada lampiran chat.

## Verifikasi

- Unity 6000.5.10f1: 109 tes Edit Mode memeriksa cerita, skor, save/replay, laporan delapan isu, URL/koneksi default, kecocokan receipt, sinyal musik dan proporsi meter.
- Konektor Gemini: 29 tes Node lulus menggunakan provider palsu untuk format Gemini, filter/keluaran parsial, timeout/error, validasi, moderasi, batas konteks, persetujuan, penerima tetap, autentikasi dan pencegahan pengiriman ganda. Tidak memerlukan kredensial nyata. Penambahan ini hanya mengubah backend/dokumentasi; hasil Unity dan Play mode di bagian ini berasal dari QA revisi dosen sebelumnya, bukan pengujian ulang Gemini melalui Editor.
- Play mode Windows: menu/tombol, HUD tanpa meter, musik/narasi dan ducking, save/resume, review → reflection, respons lokal, preview/gulir/ekspor serta tombol WhatsApp nonaktif diperiksa. Alamat loopback yang tidak tersedia dipakai untuk menguji fallback chat dan receipt berstatus belum terkonfirmasi, tanpa kontak layanan eksternal.
- Tangkapan layar dan hasil tes lokal berada di `Logs/LecturerQA/` (diabaikan Git). Save asli dibackup sebelum QA. Tidak ada pengiriman WhatsApp/AI sungguhan atau build standalone yang diverifikasi pada perubahan ini.

Skill Unity digunakan untuk pemeriksaan kompilasi, tes dan tampilan runtime; dokumentasi resmi OpenAI memandu batas data dan desain konektor yang nonaktif secara default.

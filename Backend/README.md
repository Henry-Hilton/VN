# YouthRise — konektor demo Safe Zone dan WhatsApp Guru BK

Untuk fitur terbaru register/login, Alex/Anita, dashboard hasil dan laporan terkonfirmasi, ikuti **[panduan demo lokal](LOCAL-DEMO.md)**. Alur game terbaru memakai PlayerPrefs untuk login dan dashboard dalam game; backend tidak diperlukan. Lihat [panduan terbaru](../Assets/YouthRise/Local-Features.md).

Bagian berikut mendokumentasikan endpoint konektor AI/WhatsApp lama, yang **belum diaktifkan atau dipasang di hosting** dan tidak dipakai alur demo lokal terbaru. Gunakan data fiktif untuk demo terawasi; jangan membuka server ini untuk siswa sebelum peninjauan keselamatan dan privasi.

## Fitur dan batasnya

- `POST /chat`: respons pendamping AI melalui **Gemini atau OpenAI**, dipilih pada server; konteks maksimal enam pesan, tanpa alat atau kemampuan mengirim laporan. Gemini memakai filter keselamatan bawaannya; jalur OpenAI tetap memakai moderasi masukan/keluaran terpisah. Ada respons lokal ketika layanan tidak tersedia. Ini bukan konselor manusia, diagnosis, atau layanan darurat.
- `POST /reports`: mengirim teks yang ditinjau pemain ke **satu nomor Guru BK yang ditetapkan server**, memakai WhatsApp Cloud API resmi. Tidak ada Telegram, otomatisasi WhatsApp Web, atau pengiriman otomatis dari skor pemain.
- Laporan insiden dan ringkasan delapan isu merupakan dua tindakan terpisah. Chat tidak dilampirkan ke laporan.
- Bukti lokal mencatat ID percobaan. Sesudah API mengembalikan ID pesan yang sesuai, status menjadi `accepted_by_whatsapp`. Itu **bukan** bukti delivered, read, atau ditindaklanjuti. Status pengiriman lanjutan memerlukan webhook Meta yang belum diimplementasikan. [Dokumentasi Messages milik Meta](https://www.postman.com/meta/whatsapp-business-platform/folder/o48mro7/messages).

## Menjalankan pemeriksaan lokal

Node 22.9+ diperlukan; diuji memakai Node 24.19. Tidak perlu memasang paket npm tambahan.

```powershell
Set-Location C:\Users\Henry\VN\Backend
npm test
```

Tes konektor memakai penyedia palsu dan server loopback sementara. Tidak mengirim pesan WhatsApp atau memanggil AI sungguhan. Mencakup pemilihan penyedia, format Gemini, filter, respons terpotong, konteks, timeout, kegagalan kuota, privasi error, serta regresi OpenAI/WhatsApp.

Untuk menjalankan server demo dengan layanan eksternal tetap mati:

1. Salin `.env.example` menjadi `.env`, tanpa menimpa konfigurasi yang sudah ada.
2. Isi `DEMO_ACCESS_CODE` dengan kode acak minimal 16 karakter khusus sesi demo; tetap biarkan `ENABLE_EXTERNAL_SERVICES=false`.
3. Jalankan `npm start` dari folder ini. Default hanya mendengarkan `127.0.0.1:8787`.

File `.env` dan `private-receipts/` diabaikan Git. Jangan menaruh kredensial atau data siswa dalam repositori, Unity Resources, log, atau chat pengembangan.

## Gemini: tanpa menjalankan model di komputer sendiri

Komputer hanya menjalankan game dan konektor Node yang ringan; generasi jawaban berlangsung di layanan Google. Tidak perlu GPU atau mengunduh model lokal. Gemini tidak memerlukan kunci OpenAI. Integrasi ini hanya untuk **chat Safe Zone**, bukan pengiriman WhatsApp atau perubahan PCG cerita/narasi.

**Batas penggunaan:** ketentuan Gemini saat ditinjau melarang API client yang ditujukan kepada atau kemungkinan diakses pengguna di bawah 18 tahun. Target YouthRise 11–18 belum sesuai dengan batas itu. Selain itu, pada layanan gratis, masukan/keluaran dapat digunakan untuk peningkatan produk dan ditinjau manusia; jangan kirim informasi sensitif, rahasia, atau pribadi. Persetujuan sekolah/checkbox saja tidak mengatasi batas usia; layanan berbayar juga tidak otomatis menghapusnya. Kode konektor bukan persetujuan deployment. Jangan aktifkan Gemini bagi siswa; pastikan kelayakan penggunaan dengan penyedia sebelum uji live apa pun. Tes otomatis di proyek ini tetap sepenuhnya fiktif dan tanpa Google. [Ketentuan Gemini](https://ai.google.dev/gemini-api/terms).

Persiapan konfigurasi privat:

1. Cabut/ganti kunci yang pernah dibagikan di chat, dan periksa pemakaian proyek Google. Masukkan **kunci pengganti hanya pada komputer/server sendiri**, jangan kirim kembali di percakapan. Ikuti [panduan keamanan kunci Google](https://ai.google.dev/gemini-api/docs/api-key).
2. Salin `.env.example` ke `.env` bila belum ada. Isi `AI_PROVIDER=gemini`, `GEMINI_API_KEY`, `GEMINI_MODEL`, dan `DEMO_ACCESS_CODE`. Biarkan `ENABLE_EXTERNAL_SERVICES=false` selama persiapan.
3. Pilih ID model teks yang tersedia untuk proyek di AI Studio, tanpa awalan `models/`. Model sengaja tidak dipilih otomatis: periksa ketersediaan, dukungan `generateContent`, kuota, dan biaya. Email mahasiswa tidak boleh dianggap bukti kuota API tanpa batas. Lihat [daftar model](https://ai.google.dev/gemini-api/docs/models) dan [harga/kuota gratis per model](https://ai.google.dev/gemini-api/docs/pricing).
4. Untuk uji live yang telah dinyatakan layak oleh pengelola/penyedia, ubah `ENABLE_EXTERNAL_SERVICES=true` secara lokal dan jalankan `npm start` dari folder `Backend`. Restart server setelah mengubah konfigurasi. Hanya variabel untuk penyedia yang dipilih yang digunakan; key/model kosong membuat chat online tetap ditolak.
5. Hubungkan game melalui konfigurasi URL dan persetujuan di bagian berikut. Kunci Gemini **tidak** dimasukkan pada kolom kode sesi game. Kode sesi ialah `DEMO_ACCESS_CODE` terpisah. Gunakan data fiktif, tanpa identitas atau pengaduan nyata, untuk pengujian yang diperbolehkan.

File `.env` privat tidak dibuat/diisi otomatis oleh perubahan ini. Tidak ada panggilan ke Google, pemeriksaan validitas kunci, atau aktivasi layanan nyata dalam verifikasi.

### Perilaku konektor Gemini

- Kunci dikirim melalui header `x-goog-api-key` ke domain API Google tetap, bukan query URL. Model hanya dari konfigurasi server; pemain tidak dapat mengubah penyedia/model/tools.
- Role `assistant` diterjemahkan menjadi `model`; hanya teks dan maksimal enam pesan dikirim. Tidak ada skor, laporan, nama sekolah, berkas, pencarian web, atau tools yang otomatis dilampirkan. Pesan yang diketik tetap dapat mengandung data pribadi, sehingga batas penggunaan di atas penting.
- Empat filter disetel `BLOCK_LOW_AND_ABOVE`. Blokir prompt/output, flag keselamatan, keluaran parsial/non-teks, format tak dikenal, atau respons kosong/lebih dari 900 karakter diganti pesan dukungan lokal. Thought/signature tidak ditampilkan. Filter konservatif dapat juga menolak pembahasan aman; jangan menganggapnya penilaian terhadap pemain atau melonggarkan filter untuk memaksa jawaban.
- Satu permintaan `generateContent`, tenggat 20 detik, tanpa retry otomatis dan tanpa fallback ke perusahaan lain. Error kuota/izin/model/jaringan menghasilkan error umum, lalu klien memakai respons lokal. Batas 2.048 output token termasuk ruang bagi model berpikir; jika model terpilih habis anggaran sebelum selesai, jawaban parsial tidak ditampilkan. Uji model terpilih sebelum deployment.
- Deteksi kata kunci krisis lokal tetap berjalan sebelum panggilan penyedia. Filter Gemini tidak sama dengan moderasi OpenAI dan bukan jaminan keselamatan/ketepatan klinis. Pemilihan penyedia tidak mengubah kontrak atau izin WhatsApp.
- Tidak ada riwayat chat yang disimpan oleh konektor ini, tetapi itu **bukan** jaminan tanpa retensi oleh Google. Kebijakan data penyedia tetap berlaku; tidak ada klaim `store: false` untuk Gemini.

Implementasi mengikuti [REST generateContent](https://ai.google.dev/api/generate-content) dan [pengaturan keselamatan Gemini](https://ai.google.dev/gemini-api/docs/safety-settings).

## Konfigurasi sebelum demo online

Pengelola sekolah harus menyetujui penerima, penggunaan penyedia, isi laporan, persetujuan, retensi, dan prosedur tindak lanjut. Setelah itu, administrator dapat mengisi variabel server berikut melalui penyimpanan rahasia server:

| Variabel | Isi |
| --- | --- |
| `AI_PROVIDER` | `gemini` atau `openai`; contoh baru memilih Gemini, instalasi lama tanpa variabel ini tetap OpenAI |
| `GEMINI_API_KEY` | Kunci pengganti Gemini; server saja, digunakan hanya untuk `AI_PROVIDER=gemini` |
| `GEMINI_MODEL` | ID model Gemini yang tersedia dan ditinjau; wajib diisi, tidak dipilih otomatis |
| `OPENAI_API_KEY` | Alternatif: kunci OpenAI server saja, digunakan hanya untuk `AI_PROVIDER=openai` |
| `OPENAI_MODEL` | Alternatif: model OpenAI yang tersedia dan ditinjau; tidak dipilih otomatis |
| `WHATSAPP_ACCESS_TOKEN` | Token Meta dengan izin messaging yang sesuai; server saja |
| `WHATSAPP_PHONE_NUMBER_ID` | ID nomor pengirim bisnis dari Meta, bukan nomor telepon penerima |
| `WHATSAPP_RECIPIENT` | Nomor Guru BK yang menyetujui penerimaan, format internasional angka saja |
| `WHATSAPP_GRAPH_VERSION` | Versi Graph API yang didukung aplikasi Meta, format `vNN.N` |
| `RECIPIENT_LABEL` | Label penerima yang jelas, sama persis dengan konfigurasi game |
| `DEMO_ACCESS_CODE` | Kode akses terpisah dari seluruh kunci API; ganti setelah demo |
| `RECEIPT_DIRECTORY` | Folder privat persisten, di luar web root, akses terbatas |

Server baru melakukan panggilan eksternal jika `ENABLE_EXTERNAL_SERVICES=true`. Tidak ada kredensial yang disediakan proyek ini. WhatsApp tetap membutuhkan konfigurasi Meta sendiri; kunci Gemini tidak mengaktifkannya. Flag eksternal berlaku untuk kedua fitur: jika kredensial Meta sudah ada, flag ini juga mengizinkan pengiriman laporan yang disetujui pemain. Gunakan endpoint HTTPS di belakang reverse proxy; jangan mempublikasikan HTTP loopback atau kode akses demo sebagai autentikasi produksi.

Konektor saat ini mengirim **pesan teks biasa**. WhatsApp mensyaratkan template yang disetujui di luar jendela layanan pelanggan 24 jam. Untuk demo, pastikan percakapan penerima memenuhi aturan tersebut; untuk penggunaan berkelanjutan, implementasikan alur template dan verifikasi status terlebih dahulu. Jangan menganggap respons API sukses sebagai jaminan pesan sampai. [Kebijakan WhatsApp Business](https://whatsappbusiness.com/policy/).

Pada `Assets/YouthRise/Resources/YouthRise/online-services.json`, administrator mengisi hanya:

```json
{
  "enabled": true,
  "baseUrl": "https://YOUR-APPROVED-SERVER",
  "recipientLabel": "LABEL GURU BK YANG DISETUJUI"
}
```

Ganti kedua placeholder; jangan memasukkan token ke file ini. Default bawaan tetap `enabled: false`, URL kosong. HTTP hanya diterima oleh validasi klien untuk loopback. Unity juga membatasi HTTP melalui Player Settings: jika benar-benar memerlukan demo loopback, izinkan HTTP **hanya untuk development**, atau gunakan HTTPS. Pengaturan proyek ini tidak dilonggarkan otomatis. WebGL/CORS dan perangkat selain Windows belum diuji.

Di game: **Safe Zone → Chat Pendamping → Koneksi / Privasi**. Masukkan kode sesi, lalu setujui pengiriman chat jika diinginkan. Persetujuan chat tidak otomatis menyetujui laporan. Di **Need Extra Help?**, buka **Ringkasan Pilihan 8 Isu** atau **Tinjau / Kirim** untuk insiden, baca seluruh preview, lalu centang persetujuan laporan. Tujuan tidak dapat diganti lewat formulir pemain.

## Kontrak dan penanganan kegagalan

Kedua endpoint memerlukan `Authorization: Bearer <kode-sesi>` dan JSON. Batas demo: 30 permintaan/menit per proses, badan 20 KB, tanpa CORS terbuka, tanpa log isi permintaan. Ini bukan autentikasi per siswa atau pembatasan terdistribusi.

- Chat: `{ "consent": true, "messages": [{ "role": "user", "content": "pesan fiktif" }] }`. Maksimal enam pesan, masing-masing 1.000 karakter, role hanya user/assistant. Gemini memakai tenggat 20 detik; rangkaian moderasi/generasi OpenAI maksimal 30 detik; klien menunggu maksimal 35 detik. Penarikan persetujuan membuang konteks lokal dan mengabaikan jawaban yang masih berjalan, tetapi tidak menarik data yang sudah terkirim.
- Laporan: `reportId` berformat `YR-` + 32 digit heksadesimal, `kind` bernilai `journey`/`incident`, `text` maksimal 3.500 karakter, `recipientLabel` harus sama dengan server, `consent: true`.
- Ledger server menyimpan hash isi, ID, penerima berupa label, status, dan ID pesan/waktu penerimaan jika tersedia; **bukan teks pengaduan**. Permintaan ulang dengan ID dan isi sama tidak mengirim ulang, termasuk sesudah restart. ID sama dengan isi berbeda ditolak.
- Jika koneksi terputus atau status `unknown`, periksa ID dengan pengelola. Jangan membuat preview/ID baru untuk memaksa retry: pesan sebelumnya mungkin sudah diterima API. Ledger rusak/parsial juga tidak memicu pengiriman ulang otomatis. Penyimpanan persisten satu server diperlukan; multi-instance dan rekonsiliasi operator belum tersedia.
- `Receipts/` di perangkat menyimpan metadata percobaan/status, bukan chat atau isi pengaduan. Ini catatan prototipe yang dapat diedit lokal, bukan tanda tangan digital atau bukti hukum.

## Sebelum digunakan oleh siswa sungguhan

Belum ada login per pengguna, age assurance, sistem wali/sekolah, antrean konselor manusia, SLA, audit akses, enkripsi penyimpanan aplikasi, penghapusan/retensi otomatis, template WhatsApp, webhook delivery/read, atau peninjauan profesional atas jawaban AI. Moderasi dan kata kunci tidak menjamin semua bahaya terdeteksi. Jangan mengandalkan aplikasi ini untuk keadaan darurat.

OpenAI dipanggil melalui Responses API dengan `store: false`, instruksi dukungan terbatas dan tanpa tools. `store: false` **bukan Zero Data Retention**; kebijakan penyimpanan penyedia tetap harus ditinjau. [Responses/text](https://developers.openai.com/api/docs/guides/text), [kontrol data](https://developers.openai.com/api/docs/guides/your-data).

Target game mencakup usia 11–18. Jangan memproses data pribadi anak di bawah 13 tahun atau usia persetujuan digital yang berlaku tanpa memenuhi persyaratan penyedia, termasuk Zero Data Retention yang disyaratkan untuk kelompok tersebut. Persetujuan dalam UI ini saja tidak mencukupi untuk penggunaan nyata. Ikuti peninjauan keselamatan remaja, kesesuaian usia, kebijakan sekolah/wali, dan evaluasi profesional sebelum aktivasi. [Panduan OpenAI untuk pengguna di bawah 18 tahun](https://developers.openai.com/api/docs/guides/safety-checks/under-18-api-guidance).

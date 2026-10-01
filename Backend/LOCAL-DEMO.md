> Legacy Node demo: the current game uses PlayerPrefs and an in-game dashboard with no backend requirement. See [current local setup](../Assets/YouthRise/Local-Features.md). The instructions below describe the retained optional web-backend implementation.

# YouthRise: akun pemain dan dashboard konselor lokal

Implementasi ini menjalankan **satu sekolah/demo per server lokal**. Tidak memerlukan API AI, WhatsApp, npm install, atau layanan eksternal. Gunakan profil dan pesan fiktif untuk demonstrasi.

## Jalankan

Persyaratan: Node.js 22.9+ dan Unity 6000.5.10f1.

```powershell
cd C:\Users\Henry\VN\Backend
npm run setup
npm start
```

`npm run setup` membuat satu akun **konselor**, dengan password acak. Lokasi file kredensial dicetak di terminal: `Backend/private-portal/access-konselor-*.txt`. Buka file tersebut secara lokal; jangan bagikan kepada pemain. Menjalankan setup lagi menambah akun konselor baru, tidak mereset akun lama.

Buka **http://127.0.0.1:8787/dashboard** dan gunakan kredensial konselor. Server hanya mendengarkan loopback secara default. Biarkan terminal server berjalan saat bermain.

Di Unity, buka `Assets/Scenes/SampleScene.unity`, lalu Play. Halaman login muncul terlebih dahulu. Pilih **Belum punya akun? Daftar**, isi nama, nickname unik, password, usia 11–18, dan gender, lalu setujui penyimpanan profil/hasil. Login berikutnya menggunakan nickname dan password. Nickname tidak peka huruf besar/kecil.

Endpoint Unity berada di `Assets/YouthRise/Resources/YouthRise/account-services.json`. HTTP lokal diizinkan di Editor/development build; build non-development memerlukan HTTPS. Konfigurasi AI/WhatsApp lama tetap terpisah dan nonaktif.

## Perilaku fitur

- **Laki-laki → Alex**, tokoh cerita awal. **Perempuan → Anita**, dengan portrait baru dan nama yang disesuaikan dalam dialog, pilihan dan tampilan cerita. Nama/nickname akun tetap menjadi identitas pemain; tidak mengganti nama tokoh cerita.
- Save dipisahkan berdasarkan ID akun pada `Application.persistentDataPath/YouthRise/Accounts/<id>/prototype-save.json`. Save lama yang belum memiliki akun tetap disimpan di lokasi lama dan tidak otomatis diklaim pemain baru. Progress cerita lokal tidak berpindah perangkat.
- Hasil pilihan tersinkron otomatis setelah save. Jika koneksi gagal, game menyimpan progress lokal dan mencoba kembali; status terlihat di menu. Tombol **Sinkron ulang** tersedia. Login ulang mengirim ulang hasil dari save akun. Jangan menganggap data dashboard terbaru sebelum status sinkron sukses.
- Dashboard menyediakan jumlah pemain, distribusi pilihan pada delapan tema, profil dan hasil setiap pemain, pencarian, serta kotak laporan dengan status **Baru / Sudah ditinjau / Ditutup**. Pengelola harus menekan **Perbarui data** untuk mengambil perubahan terbaru.
- Server menghitung ringkasan dari ID pilihan yang valid dalam delapan JSON chapter. Tidak mempercayai skor agregat kiriman client. Rubrik menggambarkan pilihan cerita, **bukan diagnosis atau penilaian kepribadian**. Belum ada data ditampilkan sebagai belum ada data; pengulangan chapter mengganti hasil chapter tersebut.
- **Safe Zone tersedia sejak login**. Chat biasa memakai respons lokal yang ditulis sebelumnya; tidak disimpan atau dikirim ke dashboard/AI. Ini bukan chatbot generatif dan tidak menjamin memahami semua situasi.
- Kata kunci urgensi membuka **pratinjau**, bukan mengirim otomatis. Pemain dapat kembali tanpa mengirim. Setelah menekan **Setuju, kirim ke konselor**, hanya pesan yang ditampilkan dan profil akun yang tersedia bagi konselor. Pelaporan ini **tidak anonim**; seluruh percakapan tidak ikut dikirim.
- Pemain juga dapat membuka **Need Extra Help? → Tinjau / Kirim** untuk melaporkan kejadian meski kata kunci tidak terdeteksi. Deteksi urgensi menggunakan kata kunci dan dapat salah atau terlewat.
- Status **Diterima dashboard** berarti laporan berhasil disimpan, bukan sudah dibaca/ditangani. Retry pada pratinjau yang sama memakai ID yang sama untuk menghindari duplikasi. Bila menutup aplikasi setelah hasil pengiriman tidak jelas, periksa dashboard sebelum membuat laporan baru.

## Penyimpanan dan akses

`Backend/private-portal/database.json` menyimpan profil, password hash scrypt dengan salt acak, hasil, dan laporan. Folder ini serta file akses konselor diabaikan Git. Data profil/laporan **belum dienkripsi di disk**; akses file mengikuti akun OS. Sesi ada di memori server, berakhir setelah delapan jam dan hilang saat server restart. Login kembali setelah restart. Token pemain hanya berada di memori game; sesi web memakai cookie HttpOnly/SameSite=Strict. Registrasi publik hanya dapat membuat role pemain. Semua endpoint hasil/dashboard/laporan memeriksa sesi dan role di server.

Demo ini belum menyediakan multi-sekolah, pemulihan password, MFA, notifikasi push, kebijakan retensi otomatis, atau penerapan internet. Jangan membuka port ke jaringan publik. Konselor harus membuka dashboard untuk memeriksa pesan; aplikasi bukan layanan darurat.

## Verifikasi

```powershell
cd C:\Users\Henry\VN\Backend
npm test
```

Di Unity pilih **YouthRise → QA → Run EditMode Tests** (di luar Play Mode). Hasil XML disimpan di `Temp/YouthRiseQA/editmode-results.xml`.

Untuk integration smoke test Unity, jalankan server **terpisah** dengan direktori data uji dan port berbeda, tulis URL loopback-nya ke `Logs/YouthRiseQA/portal-url.txt`, masuk Play Mode dalam keadaan logout, lalu pilih **YouthRise → QA → Run Account Integration Smoke Test**. Pengujian membuat akun/pesan fiktif di server uji dan memeriksa register, portrait Anita, save/sinkron pilihan, chat biasa, pratinjau urgensi, konfirmasi/receipt, logout dan login ulang. Hasil: `Logs/YouthRiseQA/portal-smoke.txt`. Jangan arahkan pengujian ke database demo yang dipakai pengguna.

# **2024 / 2025 - Diplomski Projekt**
### **Komunikacija i kolaboracija udaljenih korisnika primjenom tehnologija virtualne i proširene stvarnosti**

**Članovi tima:**  
Marin Petric, Robert Kunštek, Eugen Preglej, Branimir Tomeljak, Anteo Vukasović

---

### **Upute za korištenje:**

**Potrebno:**
- Oculus Quest 2 (za VR)
- Oculus Quest 3 (za AR)
- Pametna utičnica (Delock WLAN Power Socket Switch MQTT with energy monitoring)
- Lampa

Aplikacija podržava dva korisnika: VR i AR korisnika.
Testirano je da VR radi na Oculus Quest 2, dok AR funkcionalnosti rade na Oculus Quest 3, no VR bi trebao biti kompatibilan i s Quest 3.

---

### **Upute za spajanje pametne utičnice na mrežu:**

Ako utičnica još nije spojena na mrežu, slijedite ove korake:

1. Uključite utičnicu u struju i četiri puta brzo pritisnite gumb.
2. Pomoću računala povežite se na Wi-Fi mrežu pod nazivom **"delock-3530"**.
3. Ako se ne otvori automatski browser popup, otvorite `http://192.168.4.1/` u svom web pregledniku.
4. Upišite samo prva dva polja (ime i lozinka Wi-Fi mreže na koju želite povezati pametnu utičnicu). Ostala polja možete ostaviti praznima.
5. Spojite računalo na istu mrežu na koju ste povezali utičnicu i sačekajte 2-3 minute.
6. Provjerite radi li utičnica posjetom **`http://delock-3530.local/`**.

---

### **Napomena:**

Zbog problema s DNS razlučivanjem `.local` adresa na Oculus Quest 3, pametna utičnica koristi hardkodiranu IP adresu u buildanim verzijama aplikacije. Ako se IP adresa utičnice promijeni, potrebno je izvršiti sljedeće:

1. Otvorite Unity Editor.
2. Nađite prefab **`Lamp System`**.
3. Navigirajte do **`Lamp System -> Lamp Manager [Switch Control skripta] -> Base Url`**.
4. Zamijenite staru IP adresu s novom.
5. Ponovno buildajte projekt (samo AR aplikaciju).

Za pronalazak nove IP adrese utičnice na Windowsu, možete koristiti naredbe u konzoli:

- `nslookup delock-3530.local`
- `ping delock-3530.local`

---

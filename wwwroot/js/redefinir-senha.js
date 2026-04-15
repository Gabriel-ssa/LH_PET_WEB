function avaliarForcaSenhaFunc(senha) {
    let forca = 0;
    const barra = document.getElementById('barraForcaFunc');
    const texto = document.getElementById('textoForcaFunc');

    if (senha.length >= 8) forca += 1;
    if (/[A-Z]/.test(senha)) forca += 1;
    if (/[a-z]/.test(senha)) forca += 1;
    if (/\d/.test(senha)) forca += 1;
    if (/[\W_]/.test(senha)) forca += 1;

    barra.className = "progress-bar transition-all";
    
    if (senha.length === 0) {
        barra.style.width = "0%";
        texto.innerText = "";
    } else if (forca <= 2) {
        barra.style.width = "33%";
        barra.classList.add("bg-danger");
        texto.innerText = "Fraca";
        texto.className = "small fw-bold text-danger";
    } else if (forca >= 3 && forca < 5) {
        barra.style.width = "66%";
        barra.classList.add("bg-warning");
        texto.innerText = "Média";
        texto.className = "small fw-bold text-warning";
    } else if (forca === 5) {
        barra.style.width = "100%";
        barra.classList.add("bg-success");
        texto.innerText = "Forte";
        texto.className = "small fw-bold text-success";
    }
}
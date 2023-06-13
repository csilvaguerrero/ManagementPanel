//Script Comprobar nombre usuario valido en _Editar
const myField2 = document.querySelector('#UsuarioControlar2');
const myFieldError2 = document.querySelector('#error2');

myField2.addEventListener('blur', function () {
    const value = myField2.value;
    fetch(`/Login/CheckUnique?value=${value}`)
        .then(function (response) {
            return response.json();
        })
        .then(function (isUnique) {
            if (!isUnique) {
                myField2.classList.add('is-invalid');
                myField2.value = null;
                myFieldError2.textContent = 'Este nombre de usuario ya está en uso';
            } else {
                myField2.classList.remove('is-invalid');
                myFieldError2.textContent = '';
            }
        });
});
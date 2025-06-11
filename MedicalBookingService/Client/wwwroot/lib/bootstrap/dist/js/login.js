window.getInputValue = function (id) {
    return document.getElementById(id)?.value;
};

window.startSpinner = function () {
    const loader = document.getElementById("loader");
    if (loader) {
        loader.style.display = "flex";
        document.body.classList.add("spinner-active");
    }
};

window.stopSpinner = function () {
    const loader = document.getElementById("loader");
    if (loader) {
        loader.style.display = "none";
        document.body.classList.remove("spinner-active");
    }
};


window.showErrorMessage = function (message) {
    const errorDiv = document.getElementById("errorMessage");
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.display = "block";
    }
};

window.loginFunctionApi = async function (email, password) {
    const response = await fetch('/api/auth/login', {
        method: 'POST',
        credentials: 'include', // crucial to retain Identity cookie
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
    });

    const isJson = response.headers.get('content-type')?.includes('application/json');
    const body = isJson ? await response.json() : await response.text();

    return {
        status: response.status,
        data: body
    };
};

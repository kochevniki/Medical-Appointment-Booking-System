﻿window.signupEnhancements = {
    initSSNFields: function () {
        const ssnFields = ['SSN', 'ConfirmSSN'];

        ssnFields.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;

            el.addEventListener('input', function () {
                let value = el.value.replace(/\D/g, '');
                if (value.length > 9) value = value.slice(0, 9);

                if (value.length > 5) {
                    value = `${value.slice(0, 3)}-${value.slice(3, 5)}-${value.slice(5)}`;
                } else if (value.length > 3) {
                    value = `${value.slice(0, 3)}-${value.slice(3)}`;
                }

                el.value = value;
            });
        });
    },

    initPhoneNumberField: function () {
        const el = document.getElementById('PhoneNumber');
        if (!el) return;

        el.addEventListener('input', function () {
            let value = el.value.replace(/\D/g, '');
            if (value.length > 10) value = value.slice(0, 10);

            if (value.length > 6) {
                value = `${value.slice(0, 3)}-${value.slice(3, 6)}-${value.slice(6)}`;
            } else if (value.length > 3) {
                value = `${value.slice(0, 3)}-${value.slice(3)}`;
            }

            el.value = value;
        });
    },

    initAddressAutocomplete: async function (dotNetHelper) {
        const input = document.getElementById('usps-address');
        if (!input) return;

        try {
            const response = await fetch('https://localhost:7136/api/GoogleApi/key');
            if (!response.ok) throw new Error('Failed to load API key');
            const apiKey = await response.text();

            const script = document.createElement('script');
            script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places`;
            script.async = true;
            script.defer = true;
            script.onload = () => {
                const autocomplete = new google.maps.places.Autocomplete(input, {
                    types: ['address'],
                    componentRestrictions: { country: 'us' }
                });

                autocomplete.addListener('place_changed', () => {
                    const place = autocomplete.getPlace();
                    console.log('Selected address:', place.formatted_address);
                    // Optionally update hidden fields or extra data
                    if (place.formatted_address) { // <-- Important check
                        // Invoke the C# method with the selected address
                        dotNetHelper.invokeMethodAsync('UpdateAddress', place.formatted_address);
                    }
                });
            };
            document.head.appendChild(script);
        } catch (err) {
            console.error('Google Places Autocomplete failed:', err);
        }
    }
};

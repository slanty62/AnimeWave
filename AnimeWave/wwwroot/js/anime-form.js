document.addEventListener("DOMContentLoaded", function () {


    const rating =
        document.getElementById("Rating");


    const ratingValue =
        document.getElementById("ratingValue");



    if (rating) {

        rating.addEventListener("input", function () {

            if (ratingValue) {

                ratingValue.textContent =
                    Number(rating.value)
                        .toFixed(1);

            }

        });

    }





    const episodes =
        document.getElementById("EpisodesCount");


    const plus =
        document.getElementById("plusEpisode");


    const minus =
        document.getElementById("minusEpisode");



    if (plus && episodes) {

        plus.onclick = function () {

            episodes.value =
                Number(episodes.value) + 1;

        };

    }



    if (minus && episodes) {

        minus.onclick = function () {

            let value =
                Number(episodes.value);


            if (value > 0) {

                episodes.value =
                    value - 1;

            }

        };

    }





    const genres =
        document.querySelectorAll(
            ".genre-chip input"
        );


    genres.forEach(function (input) {


        input.addEventListener(
            "change",
            function () {


                const parent =
                    input.closest(".genre-chip");


                if (input.checked) {

                    parent.classList.add(
                        "active"
                    );

                }
                else {

                    parent.classList.remove(
                        "active"
                    );

                }


            }
        );


    });


});
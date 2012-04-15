var ColourSearch = (function () {

    var colourSearch = function () {
        $("#btnSearch").click(onSearch);
    };


    var onSearch = function () {

        var colour = $("#color").val().substring(1);
        //var colourSpace = $("#colourspace").val();
        var colourSpace = 'rbg';

        search(colour, colourSpace, "ChiSquare", $("#result1"));
        search(colour, colourSpace, "ChiSquare2", $("#result2"));
        search(colour, colourSpace, "Correlation", $("#result3"));
        search(colour, colourSpace, "Intersection", $("#result4"));

        return false;
    };

    var search = function (colour, colourSpace, compareMethod, resultDiv) {
        resultDiv.html("");

        resultDiv.append("<h2>" + compareMethod + "</h2>");

        $.getJSON("http://localhost:9200/search/?callback=?",
            {
                colour: colour,
                page: 0,
                pageSize: 100,
                colourSpace: colourSpace,
                compareMethod: compareMethod
            },
            function (response) {

                for (var i = 0; i < response.result.length; i++) {
                    var image = response.result[i];
                    resultDiv.append("<img src='/Content/Images/" + image.filename + "' width='200' title='" + image.distance + "' />");
                }
            });
    };


    return colourSearch;
})();


$(document).ready(function() {
    var search = new ColourSearch();
});
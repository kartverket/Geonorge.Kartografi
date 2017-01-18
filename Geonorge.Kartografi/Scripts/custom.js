var metadata = [];
$('.datasetUuidSelect').select2({
    placeholder: "Søk etter datasett",
    language: "nb",
    ajax: {
        url: kartkatalogenUrl+ "api/search",
        dataType: 'json',
        delay: 250,
        data: function (params) {
            return {
                text: params.term,// search term
                limit: 10,
                'facets[0]name': "type",
                'facets[0]value': "dataset"
            };
        },
        processResults: function (data, params) {

            metadata = [];
            $.each(data.Results, function (i, item) {
                option = {}
                option["id"] = item.Uuid;
                option["text"] = item.Title;
                option["theme"] = item.Theme;

                metadata.push(option);
            })

            return {
                results: metadata
            };
        },
        cache: true
    },
    minimumInputLength: 3
});

$('.datasetUuidSelect').on('select2:select', function (evt) {

    var uuidSelected = $("#DatasetUuid").val();

    $.each(metadata, function (i, item) {
        if (uuidSelected == item.id) {
            $("#DatasetName").val(item.text);
            $("#Theme").val(item.theme);

            $('#ServiceUuid').empty();
            $.getJSON(kartkatalogenUrl+ "api/getdata/" + uuidSelected, function (relatedData) {
                if (relatedData.length != 0) {
                    $('#ServiceUuid').append($("<option></option>")
                                .attr("value", "").text("Velg tjeneste"));
                    for (r = 0 ; r < relatedData.Related.length; r++) {
                        var related = relatedData.Related[r];
                        var distributionDetails = related.DistributionDetails;
                        if (distributionDetails != null) {
                            if (distributionDetails.Name == "" && (distributionDetails.Protocol == "OGC:WMS" || distributionDetails.Protocol == "OGC:WFS")) {
                                $('#ServiceUuid').append($("<option></option>")
                                    .attr("value", related.Uuid).text(related.Title));
                            }
                        }
                    }
                }
            });
        }
    })

});

$('#ServiceUuid').on('change', function () {
    $('#ServiceName').val($('#ServiceUuid option:selected').text());
})

var organizations = [];

$('.ownerOrganizationSelect').select2({
    placeholder: "Søk etter organisasjon",
    language: "nb",
    ajax: {
        url: registryUrl + "api/search",
        dataType: 'json',
        delay: 250,
        data: function (params) {
            return {
                text: params.term,// search term
                limit: 10,
                'facets[0]name': "type",
                'facets[0]value': "organisasjoner"
            };
        },
        processResults: function (data, params) {
            organizations = [];
            $.each(data.Results, function (i, item) {
                option = {}
                option["id"] = item.Name;
                option["text"] = item.Name;

                organizations.push(option);
            })

            return {
                results: organizations
            };
        },
        cache: true
    },
    minimumInputLength: 3
});

$(".compatibilitySelect").select2();
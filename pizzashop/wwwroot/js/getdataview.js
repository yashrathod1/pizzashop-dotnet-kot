$(document).ready(function () {
    var selectedCountryId = $("#Country").attr("data-selected-id");
    var selectedStateId = $("#State").attr("data-selected-id");
    var selectedCityId = $("#City").attr("data-selected-id");

   
    $.ajax({
        url: "/GetData/GetCountries",
        type: "GET",
        success: function (data) {
            $("#Country").empty().append('<option value="">Select Country</option>');
            $.each(data, function (index, country) {
                $("#Country").append('<option value="' + country.countryId + '">' + country.countryName + '</option>');
            });

            
            if (selectedCountryId) {
                $("#Country").val(selectedCountryId).change(); 
            }
        }
    });

    
    $("#Country").change(function () {
        var countryId = $(this).val();
        $("#State").empty().append('<option value="">Select State</option>');
        $("#City").empty().append('<option value="">Select City</option>'); 

        if (!countryId) return;

        $.ajax({
            url: "/GetData/GetStates",
            type: "GET",
            data: { countryId: countryId },
            success: function (data) {
                $.each(data, function (index, state) {
                    $("#State").append('<option value="' + state.stateId + '">' + state.stateName + '</option>');
                });

                
                if (selectedStateId && $("#State option[value='" + selectedStateId + "']").length) {
                    $("#State").val(selectedStateId).change();
                } else {
                    selectedStateId = null;
                }
            }
        });
    });

    
    $("#State").change(function () {
        var stateId = $(this).val();
        $("#City").empty().append('<option value="">Select City</option>');

        if (!stateId) return;

        $.ajax({
            url: "/GetData/GetCities",
            type: "GET",
            data: { stateId: stateId },
            success: function (data) {
                $.each(data, function (index, city) {
                    $("#City").append('<option value="' + city.cityId + '">' + city.cityName + '</option>');
                });

            
                if (selectedCityId && $("#City option[value='" + selectedCityId + "']").length) {
                    $("#City").val(selectedCityId);
                } else {
                    selectedCityId = null; 
                }
            }
        });
    });

    $("#State").click(function () {
        if (!$("#Country").val()) {
            toastr.warning("Please select a Country first!");
            $("#State").val(""); 
        }
    });

    $("#City").click(function () {
        if (!$("#State").val()) {
            toastr.warning("Please select a state first!");
            $("#City").val(""); 
        }
    });
});


function loadEditFormHarvest(harvestId) {
    $.ajax({
        url: "/Harvest/Edit/" + harvestId,
        type: "GET",
        success: function (data) {
            // Update the content inside the container with the edit form
            $("#harvestDetailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error loading edit form:", error);
        }
    });
}

// Details button click event
function redirectToDetails(harvestId) {
    $.ajax({
        url: "/Harvest/Details/" + harvestId,
        type: "GET",
        success: function (data) {
            // Update the content inside the container with the edit form
            $("#harvestDetailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error loading details form:", error);
        }
    });
}

function deleteHarvest(harvestId) {
    $.ajax({
        url: "/Harvest/Delete/" + harvestId,
        type: "GET",  // Change the request type to "POST"
        success: function (data) {
            // Update the content inside the container with the response data
            $("#harvestDetailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error performing delete operation:", error);
        }
    });
}

$(function () {
    $("#plantClassAutocomplete").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/Vegetation/GetVegetations",
                type: "GET",
                data: { term: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.label,
                            value: item.label,
                            id: item.id
                        };
                    }));
                }
            });
        },
        minLength: 1, // Set the minimum length for triggering autocomplete
        select: function (event, ui) {
            // Set the selected id to the hidden input field
            $("#selectedVegetationId").val(ui.item.id);
        }
    });
});




document.getElementById('saveHarvestBtn').addEventListener('click', function (event) {
    // Prevent the default form submission
    event.preventDefault();
    // Submit the form using JavaScript
    document.getElementById('harvestForm').submit();
});







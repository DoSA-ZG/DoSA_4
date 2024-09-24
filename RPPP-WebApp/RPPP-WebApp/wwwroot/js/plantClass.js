// Function to handle the GET request and display the form
function getPlantForm(recipeId) {
    // Your AJAX request for the GET method
    $.ajax({
        url: "/Ingredient/Create",
        data: { recipeId: recipeId },
        method: "GET",
        success: function (data) {
            

            // For example, you can update a container with the form content
            $('#plantsContainer').html(data);
        },
        error: function (error) {
            // Handle the error response
            console.error('Error loading form:', error);
        }
    });
}

function showDetailsForm(plantId) {

    // Your AJAX request
    $.ajax({
        url: '/PlantClass/Details/' + plantId, // Update the URL with your controller action
        data: { plantId: plantId },
        method: 'GET',
        success: function (data) {
            $('#plantsContainer').html(data);
        },
        error: function (error) {
            // Handle the error response
            console.error('Error loading plant details:', error);
        }
    });
}
function showEditForm(plantId) {

    // Your AJAX request
    $.ajax({
        url: '/PlantClass/Edit/' + plantId, // Update the URL with your controller action
        data: { plantId: plantId },
        method: 'GET',
        success: function (data) {
            $('#plantsContainer').html(data);
        },
        error: function (error) {
            // Handle the error response
            console.error('Error loading plant details:', error);
        }
    });
}
function showDeleteForm(plantId) {

    // Your AJAX request
    $.ajax({
        url: '/PlantClass/Delete/' + plantId, // Update the URL with your controller action
        data: { plantId: plantId },
        method: 'GET',
        success: function (data) {
            $('#plantsContainer').html(data);
        },
        error: function (error) {
            // Handle the error response
            console.error('Error loading plant details:', error);
        }
    });
}
$(function () {
    $("#recipeAutocomplete").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/Ingredient/GetRecipes",
                type: "GET",
                data: { term: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.label, // The label to be displayed in the autocomplete
                            value: item.label, // The value to be filled in the input when an item is selected
                            id: item.id // The id associated with the selected item
                        };
                    }));
                }
            });
        },
        minLength: 2, // Set the minimum length for triggering autocomplete
        select: function (event, ui) {
            // Set the selected id to the hidden input field
            $("#selectedRecipeId").val(ui.item.id);
        }
    });
});







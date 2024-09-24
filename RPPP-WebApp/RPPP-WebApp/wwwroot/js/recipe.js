

function showEditRecipeForm(recipeId) {

    // Your AJAX request
    $.ajax({
        url: '/Recipe/Edit/' + recipeId, // Update the URL with your controller action
        data: { recipeId: recipeId },
        method: 'GET',
        success: function (data) {
            $('#recipeContainer').html(data);
        },
        error: function (error) {
            // Handle the error response
            console.error('Error loading plant details:', error);
        }
    });
}
function showDeleteRecipeForm(recipeId) {

    // Your AJAX request
    $.ajax({
        url: '/Recipe/Delete/' + recipeId, // Update the URL with your controller action
        data: { recipeId: recipeId },
        method: 'GET',
        success: function (data) {
            $('#recipeContainer').html(data);
        },
        error: function (error) {
            // Handle the error response
            console.error('Error loading plant details:', error);
        }
    });
}






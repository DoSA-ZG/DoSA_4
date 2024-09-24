// wwwroot/js/purchaseActions.js

// Edit button click event
function loadEditForm(purchaseId) {
    $.ajax({
        url: "/Purchase/Edit/" + purchaseId,
        type: "GET",
        success: function (data) {
            // Update the content inside the container with the edit form
            $("#detailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error loading edit form:", error);
        }
    });
}

// Details button click event
function redirectToDetails(purchaseId) {
    $.ajax({
        url: "/Purchase/Details/" + purchaseId,
        type: "GET",
        success: function (data) {
            // Update the content inside the container with the edit form
            $("#detailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error loading details form:", error);
        }
    });
}

function deletePurchase(purchaseId) {
    $.ajax({
        url: "/Purchase/Delete/" + purchaseId,
        type: "GET",
        success: function (data) {
            // Update the content inside the container with the response data
            $("#detailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error performing delete operation:", error);
        }
    });
}


// Update your addNewPurchase function
function addNewPurchase(harvestId) {
    $.ajax({
        url: "/Purchase/CreateForm",
        data: { harvestId: harvestId },
        type: "GET",
        success: function (data) {
            // Update the content inside the container with the edit form
            $("#detailsContainer").html(data);
        },
        error: function (error) {
            console.error("Error loading create form:", error);
        }
    });
}

document.getElementById('savePurchaseBtn').addEventListener('click', function (event) {
    // Prevent the default form submission
    event.preventDefault();
    // Submit the form using JavaScript
    document.getElementById('purchaseForm').submit();
});







$(document).on('click', '.delete-measure', function (e) {
    e.preventDefault();
    const tr = $(this).parents("tr");
    tr.remove();
});

const getMaxIndex = () => {
    const indexes = $('.measure-index');
    let maxIndex = 0;
    for (const idx of indexes) {
        const value = parseInt(idx.value);
        if (maxIndex < value) {
            maxIndex = value;
        }
    }
    return maxIndex;
}

$(document).on('click', '#add-measure', function (e) {
    e.preventDefault();
    const nextIndex = getMaxIndex() + 1;
    let template = $('#new-measure-template').html();

    template = template.replace(/--index--/g, nextIndex);
    const lastRow = $('#table-measures').find('tr').last();
    $(template).find('tr').insertAfter(lastRow);
});

const measureExists = (measureId) => {
    const ids = $('.measure-id');
    for (const id of ids) {
        if (measureId == id.value) {
            return true;
        }
    }
    return false;
};

const assignMeasure = (measure) => {
    const nextIndex = getMaxIndex() + 1;
    let template = $('#existing-measure-template').html();

    template = template.replace(/--index--/g, nextIndex)
    .replace(/--id--/g, measure.idMeasure)
    .replace(/--datetime--/g, measure.performedOn)
    .replace(/--description--/g, measure.description ?? '')
    .replace(/--duration--/, measure.durationMinutes ?? '');

    const lastRow = $('#table-measures').find('tr').last();
    const inserted = $(template).find('tr').insertAfter(lastRow);

    $(inserted).find(`.select-measure-type`).val(measure.measureTypeId);
    $(inserted).find(`.select-measure-vegetation`).val(measure.vegetationId);
};

$(document).on('click', '#assign-measure', function (e) {
    e.preventDefault();
    const id = $('#assign-id').val();
    if (id === '') {
        toastr.error("Please specify an ID");
        return;
    }

    if (measureExists(id)) {
        toastr.error("This measure is already included");
        return;
    }
    
    $.ajax({
        url: '/api/measures/' + id,
        type: 'GET',
        success: function (data) {
            assignMeasure(data);
        },
        error: function () {
            toastr.error("A measure with this ID does not exist");
        }
    });
});
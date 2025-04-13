
$(document).ready(function () {

    loadTaxesAndFees();

});


$("#searchTaxes").on("keyup", function () {
    clearTimeout(this.delay);
    this.delay = setTimeout(function () {
        searchTaxes();
    }, 300);
});

function searchTaxes() {
    let searchTerm = $("#searchTaxes").val().trim();
    loadTaxesAndFees(1, 5, searchTerm);
}

function loadTaxesAndFees(pageNumber = 1, pageSize = 5, searchTerm = "") {
    $.ajax({
        url: "/TaxesAndFees/GetTaxesAndFees",
        type: "GET",
        data: { pageNumber: pageNumber, pageSize: pageSize, searchTerm: searchTerm },
        success: function (response) {
            $("#taxesAndFeesListContainer").html(response);
            $("#taxesPerPage").val(pageSize);
     

            $(".taxespagination-link").removeClass("active");
            $(".taxespagination-link[data-page='" + pageNumber + "']").addClass("active");
        },

        error: function () {
            toastr.error("Error fetching TaxesAndFees.");
        }
    });
    
    }
    // Handle pagination button clicks
    $(document).on("click", ".taxespagination-link", function (e) {
        e.preventDefault();
        let pageNumber = $(this).data("page");
        let pageSize = $("#taxesPerPage").val();
        loadTaxesAndFees(pageNumber, pageSize);
    });

    // Handle items per page dropdown
    $(document).on("change", "#taxesPerPage", function () {
        loadTaxesAndFees(1, $(this).val());
    });


    //addtax
    $("#addTaxForm").submit(function (e) {
        e.preventDefault();
        
        if (!$(this).valid()) {
            return; 
        }

        var formData = new FormData(this);
    
        $.ajax({
            url: '/TaxesAndFees/AddTax',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
    
                if (response.success) {
                    toastr.success("Tax added successfully");
                    $("#addTaxModal").modal("hide");
                    $("#addTaxForm")[0].reset();
                    let pageSize = $("#taxesPerPage").val();
                    let pageNumber = $(".taxespagination-link.active").data("page");
    
                    loadTaxesAndFees(pageNumber, pageSize);
                } else {
                    toastr.error("Failed to Add Tax");
                }
            },
            error: function () {
                toastr.error("Something went wrong");
            }
        });
    });

    //taxedit

    $(document).on("click", ".edit-tax", function () {
        let taxId = $(this).data("id");
    
       
    
        $.ajax({
            url: '/TaxesAndFees/GetTaxById',
            type: 'GET',
            data: { id: taxId },
            success: function (tax) {
              
    
                $("#editTaxId").val(tax.id);
                $("#editName").val(tax.name);
                $("#editType").val(tax.type);
                $("#editValue").val(tax.value);
                $("#editIsEnabled").prop("checked", tax.isEnabled);
                $("#editIsDefault").prop("checked", tax.isDefault);
              
                $('#editTaxModal').modal('show');
               
            },
            error: function () {
                alert("Failed to load tax data.");
            }
        });
    });
    
    
    $("#editTaxForm").submit(function (e) {
        e.preventDefault();

        if (!$(this).valid()) {
            return;
        }

        let taxData = {
            Id: $("#editTaxId").val(),
            Name: $("#editName").val(),
            Type: $("#editType").val(),
            Value: $("#editValue").val(),
            IsEnabled: $("#editIsEnabled").prop("checked"),
            IsDefault: $("#editIsDefault").prop("checked")
        };
    
        $.ajax({
            url: '/TaxesAndFees/EditTax',
            type: 'POST',
            contentType: "application/json",
            data: JSON.stringify(taxData),
            success: function (response) {
                if(response.success){
                    toastr.success(response.message);
                    $("#editTaxModal").modal('hide');
                    let pageSize = $("#taxesPerPage").val();
                    let pageNumber = $(".taxespagination-link.active").data("page");
    
                    loadTaxesAndFees(pageNumber, pageSize);
    
                }else{
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("Failed to update Tax.");
            }
        });
    });
    
    
    
    
    
    // @* delete item *@
    
    $(document).on("click", ".delete-tax", function () {
        var taxId = $(this).data("id");
        $("#confirmDeleteBtnTax").data("id", taxId);
        $("#deleteTaxModal").modal("show");
    
    });
    
    $("#confirmDeleteBtnTax").click(function () {
        var taxId = $(this).data("id");
        deleteTax(taxId);
        $("#deleteTaxModal").modal("hide");
    });
    
    function deleteTax(taxId) {
        $.ajax({
            url: "/TaxesAndFees/DeleteTax",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: JSON.stringify({ id: taxId }),
            success: function (response) {
                if (response.success) {
                    toastr.success("Tax deleted successfully");
                    let pageSize = $("#taxesPerPage").val();
                    let pageNumber = $(".taxespagination-link.active").data("page");
    
                    loadTaxesAndFees(pageNumber, pageSize);
                } else {
                    toastr.error("Failed to delete Tax");
                }
            },
            error: function () {
                toastr.error("Error deleting Tax");
            }
        });
    }

    
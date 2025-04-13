
function loadOrders(categoryId, targetSelector, status) {
    $.ajax({
        url: '/KOT/GetOrderCardByCategory',
        type: 'GET',
        data: { categoryId: categoryId, status: status },
        success: function (res) {
            $(targetSelector).html(res);
            initializeSlider($(targetSelector).find('.sliderContainer'));
        }
    });
}


function initializeSlider($sliderContainer) {

    let currentSlide = 0;
    const cardWidth = 407;
    const cardsToShow = 1;
    const maxSlide = $sliderContainer.children().length - cardsToShow;

    const $tab = $sliderContainer.closest('.tab-pane');

    const $nextBtn = $tab.find('.nextBtn');
    const $prevBtn = $tab.find('.prevBtn');

    $nextBtn.off('click').on('click', function () {
        if (currentSlide < maxSlide) currentSlide++;
        else currentSlide = 0;
        const offset = -currentSlide * cardWidth;
        $sliderContainer.css('transform', `translateX(${offset}px)`);
    });

    $prevBtn.off('click').on('click', function () {
        if (currentSlide > 0) currentSlide--;
        else currentSlide = maxSlide;
        const offset = -currentSlide * cardWidth;
        $sliderContainer.css('transform', `translateX(${offset}px)`);
    });
}


$(document).ready(function () {

    // Load initial tab content
    const initialStatus = $('#sliderContainer').closest('.tab-pane').find('.status-btn.active').data('status');
    loadOrders(null, '#sliderContainer', initialStatus);

    // Handle navigation clicks (Next/Previous buttons)
    $(document).on('click', '.prevBtn, .nextBtn', function () {
        return;
    });

    // Handle tab change (click on category tabs)
    $(document).on('click', '.nav-link', function () {
        const targetId = $(this).data('bs-target'); // e.g. #tab-2
        const $target = $(targetId);
        const $container = $target.find('.order-card-container');
        const categoryId = $container.data('category-id') || null;

        // Get the status from the active button in the current tab
        const status = $target.find('.status-btn.active').data('status');

        const containerSelector = $container.length > 0
            ? `${targetId} .order-card-container`
            : '#sliderContainer';
        loadOrders(categoryId, containerSelector, status);
    });

    $(document).on('click', '.status-btn', function () {
        const $this = $(this);
        const $tab = $this.closest('.tab-pane');
        $tab.find('.status-btn').removeClass('active');
        $this.addClass('active');
    
        const status = $this.data('status');
    
        if ($tab.attr('id') === 'all') {
            const containerSelector = '#sliderContainer';
            console.log("Using sliderContainer for 'All' tab", containerSelector);
            loadOrders(null, containerSelector, status);
        } else {
            const categoryId = $tab.find('.order-card-container').data('category-id');
            const containerSelector = `#${$tab.attr('id')} .order-card-container`;
            console.log("Using category-specific container", containerSelector);
            loadOrders(categoryId, containerSelector, status);
        }
    });
});





$(document).on('click', '.quantity-increase', function () {
    let box = $(this).closest('.quantity-box');
    let valueElem = box.find('.quantity-value');
    let current = parseInt(valueElem.text());
    let max = parseInt(box.data('max'));
    if (current < max) {
        valueElem.text(current + 1);
    }
});

$(document).on('click', '.quantity-decrease', function () {
    let box = $(this).closest('.quantity-box');
    let valueElem = box.find('.quantity-value');
    let current = parseInt(valueElem.text());
    let min = parseInt(box.data('min'));
    if (current > min) {
        valueElem.text(current - 1);
    }
});



$(document).ready(function () {
    $(document).on('click', '.order-card', function () {
        const orderId = $(this).data('order-id');

        $('#orderModalLabel').text('Order ID: #' + orderId);

        $.ajax({
            url: '/KOT/GetOrderCardInModal',
            type: 'GET',
            data: { orderId },
            success: function (result) {
                $('#orderModalBody').html(result);
                $('#orderModal').modal('show');
            },
            error: function () {
                $('#orderModalBody').html('<div class="text-danger">Error loading order details.</div>');
                $('#orderModal').modal('show');
            }
        });
    });
});



$(document).on('click', '#markReadyBtn', function () {
    const orderIdText = $('#orderModalLabel').text();
    const orderId = parseInt(orderIdText.replace('Order ID: #', ''));

    const items = [];

    $('#orderModalBody .order-item-row').each(function () {
        const itemId = $(this).data('item-id');
        const preparedQty = parseInt($(this).find('.quantity-value').text());

        items.push({
            itemId: itemId,
            preparedQuantity: preparedQty
        });
    });

    $.ajax({
        url: '/KOT/UpdatePreparedQuantities',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ orderId: orderId, items: items }),
        success: function () {
            $('#orderModal').modal('hide');

            const activeBtn = $('.tab-pane.active .status-btn.active').data('status');
            const currentTab = $('.tab-pane.active');
            const categoryId = currentTab.find('.order-card-container').data('category-id');
            loadOrders(categoryId, `${currentTab.selector} .order-card-container`, activeBtn);
        },
        error: function () {
            alert("Error updating order!");
        }
    });
});



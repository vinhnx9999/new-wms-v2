
function changeFloor(selectedLevel) {
   
    document.querySelectorAll('.map-layer').forEach(layer => {
        layer.classList.add('hidden');
    });

  
    const targetMap = document.getElementById(`map-level-${selectedLevel}`);
    if (targetMap) {
        targetMap.classList.remove('hidden');
    }

    
    document.querySelectorAll('.lot-container').forEach(lot => {
        if (lot.getAttribute('data-level') === selectedLevel.toString()) {
            lot.style.display = 'block'; 
        } else {
            lot.style.display = 'none';  

            
            const body = lot.querySelector('[id^="body-"]');
            const arrow = lot.querySelector('[id^="arrow-"]');
            const header = lot.querySelector('[id^="header-"]');
            if (body && !body.classList.contains('hidden')) {
                body.classList.add('hidden');
                if (arrow) arrow.classList.add('-rotate-90');
                if (header) header.classList.remove('bg-blue-50');
            }
        }
    });

    clearAllHighlights();
}

// 2. Logic Accordion
function toggleAccordion(lotId) {
    const body = document.getElementById(`body-${lotId}`);
    const arrow = document.getElementById(`arrow-${lotId}`);
    const header = document.getElementById(`header-${lotId}`);

    if (body && body.classList.contains('hidden')) {
        body.classList.remove('hidden');
        if (arrow) arrow.classList.remove('-rotate-90');
        if (header) header.classList.add('bg-blue-50');
    } else if (body) {
        body.classList.add('hidden');
        if (arrow) arrow.classList.add('-rotate-90');
        if (header) header.classList.remove('bg-blue-50');
    }
}


function clearAllHighlights() {
    document.querySelectorAll('[id^="dot-"]').forEach(dot => dot.classList.add('hidden'));
    document.querySelectorAll('.prod-item').forEach(el => {
        el.classList.remove('bg-orange-50', 'border-orange-400');
        el.classList.add('border-transparent');
    });
}

function highlightRack(addressCode, element) {
    clearAllHighlights();

    const targetDot = document.getElementById(`dot-${addressCode}`);
    if (targetDot) targetDot.classList.remove('hidden');

    if (element) {
        element.classList.remove('border-transparent');
        element.classList.add('bg-orange-50', 'border-orange-400');
    }
}


function initWarehouseMap() {
    const floorSelect = document.getElementById('floorSelect');
    if (floorSelect && floorSelect.value) {
        changeFloor(floorSelect.value);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    initWarehouseMap();
});
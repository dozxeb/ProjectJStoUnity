function moveForward() {
    const avatar = document.getElementById('player-avatar');
    avatar.style.transform = 'translateX(10px)';
    setTimeout(() => {
        avatar.style.transform = 'translateX(0)';
        nextDay();
    }, 300);
}

// O'yin o'zgaruvchilari
let gameState = {
    player: null,
    day: 1,
    money: 100,
    seeds: 10,
    farm: Array(15).fill(null).map(() => ({ crop: null, growth: 0 })),
    inventory: [],
    breedingMode: false
};

// Klaviatura boshqaruvini sozlash
function setupKeyboardControls() {
    document.addEventListener('keydown', (event) => {
        if (gameState.breedingMode) return; // Agar chatishtirish rejimida bo'lsa, boshqaruvni o'chirib qo'yamiz
        
        switch(event.key.toLowerCase()) {
            case 'w':
                moveForward();
                break;
            case 'g':
                openPlantingMenu();
                break;
            case 'v':
                if (gameState.inventory.length >= 2) {
                    breedPlants();
                } else {
                    alert("Chatishtirish uchun kamida 2 xil hosil kerak!");
                }
                break;
        }
    });
}

// Oldinga harakatlanish
function moveForward() {
    // Bu yerda foydalanuvchi interfeysini yangilash yoki animatsiya qo'shishingiz mumkin
    console.log("Oldinga harakatlanildi");
    // Keyingi kunni o'tkazish
    nextDay();
}

// Ekish menyusini ochish
function openPlantingMenu() {
    const plantingMenu = document.createElement('div');
    plantingMenu.id = 'planting-menu';
    plantingMenu.style.position = 'fixed';
    plantingMenu.style.top = '50%';
    plantingMenu.style.left = '50%';
    plantingMenu.style.transform = 'translate(-50%, -50%)';
    plantingMenu.style.backgroundColor = 'white';
    plantingMenu.style.padding = '20px';
    plantingMenu.style.border = '2px solid green';
    plantingMenu.style.borderRadius = '10px';
    plantingMenu.style.zIndex = '1000';
    
    plantingMenu.innerHTML = `
        <h3>O'simlik ekish</h3>
        <button onclick="plantCrop('bugdoy'); closeMenu('planting-menu')">Bug'doy ekish</button>
        <button onclick="plantCrop('sabzi'); closeMenu('planting-menu')">Sabzi ekish</button>
        <button onclick="plantCrop('kartoshka'); closeMenu('planting-menu')">Kartoshka ekish</button>
        <button onclick="closeMenu('planting-menu')">Bekor qilish</button>
    `;
    
    document.body.appendChild(plantingMenu);
}

// Menyuni yopish
function closeMenu(menuId) {
    const menu = document.getElementById(menuId);
    if (menu) {
        menu.remove();
    }
}

// O'simliklar ma'lumotlari
const crops = {
    bugdoy: {
        name: "Bug'doy",
        image: "https://cdn-icons-png.flaticon.com/512/2153/2153788.png",
        growthTime: 3,
        value: 20,
        seedCost: 5,
        price: 10
    },
    sabzi: {
        name: "Sabzi",
        image: "https://cdn-icons-png.flaticon.com/512/2489/2489753.png",
        growthTime: 1,
        value: 30,
        seedCost: 30,
        price: 15
    },
    kartoshka: {
        name: "Kartoshka",
        image: "https://cdn-icons-png.flaticon.com/512/2489/2489828.png",
        growthTime: 4,
        value: 50,
        seedCost: 7,
        price: 20
    }
};

// O'yinni boshlash
function startGame(gender) {
    gameState.player = {
        gender,
        name: gender === 'boy' ? "Fermer Ali" : "Fermer Zaynab"
    };
    
    document.getElementById('player-avatar').src = gender === 'boy' ? 
        "https://cdn-icons-png.flaticon.com/512/4140/4140047.png" : 
        "https://cdn-icons-png.flaticon.com/512/4140/4140048.png";
    
    document.getElementById('player-name').textContent = gameState.player.name;
    
    document.getElementById('character-selection').classList.add('hidden');
    document.getElementById('game-screen').classList.remove('hidden');
    
    renderFarm();
    updateResources();
}

// Fermani chizish
function renderFarm() {
    const farmLand = document.getElementById('farm-land');
    farmLand.innerHTML = '';
    
    gameState.farm.forEach((plot, index) => {
        const plotElement = document.createElement('div');
        plotElement.className = 'plot' + (plot.crop ? ' planted' : '');
        plotElement.onclick = () => interactWithPlot(index);
        
        if (plot.crop) {
            const cropInfo = crops[plot.crop];
            
            const cropImg = document.createElement('img');
            cropImg.className = 'crop';
            cropImg.src = cropInfo.image;
            cropImg.alt = cropInfo.name;
            
            const growthBar = document.createElement('div');
            growthBar.className = 'growth';
            
            const growthProgress = document.createElement('div');
            growthProgress.className = 'progress';
            growthProgress.style.width = `${(plot.growth / cropInfo.growthTime) * 100}%`;
            
            growthBar.appendChild(growthProgress);
            plotElement.appendChild(cropImg);
            plotElement.appendChild(growthBar);
            
            if (plot.growth >= cropInfo.growthTime) {
                plotElement.style.backgroundColor = '#FFD700';
            }
        }
        
        farmLand.appendChild(plotElement);
    });
}

// O'simlik ekish
function plantCrop(cropType) {
    const crop = crops[cropType];
    
    if (gameState.seeds < crop.seedCost) {
        alert("Sizda yetarli don yo'q!");
        return;
    }
    
    if (gameState.money < crop.price) {
        alert("Sizda yetarli pul yo'q!");
        return;
    }
    
    const emptyPlotIndex = gameState.farm.findIndex(plot => !plot.crop);
    if (emptyPlotIndex === -1) {
        alert("Fermangizda bo'sh joy yo'q!");
        return;
    }
    
    gameState.farm[emptyPlotIndex] = { crop: cropType, growth: 0 };
    gameState.seeds -= crop.seedCost;
    gameState.money -= crop.price;
    
    updateResources();
    renderFarm();
}

// Plot bilan interaksiya
function interactWithPlot(index) {
    const plot = gameState.farm[index];
    
    if (!plot.crop) return;
    
    const cropInfo = crops[plot.crop];
    
    if (plot.growth >= cropInfo.growthTime) {
        // Hosilni yig'ish
        gameState.money += cropInfo.value;
        gameState.seeds += Math.floor(cropInfo.seedCost * 1.5);
        gameState.farm[index] = { crop: null, growth: 0 };
        
        // Inventarga qo'shish
        addToInventory(plot.crop);
        
        updateResources();
        renderFarm();
    } else {
        alert(`Bu ${cropInfo.name} hali yetilmagan! ${cropInfo.growthTime - plot.growth} kun qoldi.`);
    }
}

// Keyingi kun
function nextDay() {
    gameState.day++;
    document.getElementById('day').textContent = gameState.day;
    
    // O'simliklarni o'stirish
    gameState.farm.forEach(plot => {
        if (plot.crop) {
            plot.growth += 1;
        }
    });
    
    renderFarm();
}

// Barcha hosilni yig'ib olish
function harvestAll() {
    let harvested = false;
    
    gameState.farm.forEach((plot, index) => {
        if (plot.crop && plot.growth >= crops[plot.crop].growthTime) {
            const cropInfo = crops[plot.crop];
            gameState.money += cropInfo.value;
            gameState.seeds += Math.floor(cropInfo.seedCost * 1.5);
            addToInventory(plot.crop);
            gameState.farm[index] = { crop: null, growth: 0 };
            harvested = true;
        }
    });
    
    if (harvested) {
        updateResources();
        renderFarm();
    } else {
        alert("Yig'ib olish uchun tayyor hosil yo'q!");
    }
}

// Inventarni yangilash
function updateResources() {
    document.getElementById('money').textContent = gameState.money;
    document.getElementById('seeds').textContent = gameState.seeds;
    
    const inventoryItems = document.getElementById('inventory-items');
    inventoryItems.innerHTML = '';
    
    const inventoryCount = {};
    gameState.inventory.forEach(item => {
        inventoryCount[item] = (inventoryCount[item] || 0) + 1;
    });
    
    for (const [item, count] of Object.entries(inventoryCount)) {
        const itemElement = document.createElement('div');
        itemElement.className = 'inventory-item';
        
        const img = document.createElement('img');
        img.src = crops[item].image;
        img.alt = crops[item].name;
        
        const text = document.createElement('span');
        text.textContent = `${crops[item].name}: ${count}`;
        
        itemElement.appendChild(img);
        itemElement.appendChild(text);
        inventoryItems.appendChild(itemElement);
    }
}

// Inventarga qo'shish
function addToInventory(item) {
    gameState.inventory.push(item);
}

// O'simliklarni chatishtirish
function breedPlants() {
    if (gameState.money < 50) {
        alert("Chatishtirish uchun 50 pul kerak!");
        return;
    }
    
    if (gameState.inventory.length < 2) {
        alert("Chatishtirish uchun kamida 2 xil hosil kerak!");
        return;
    }
    
    gameState.breedingMode = true;
    document.getElementById('game-screen').classList.add('hidden');
    document.getElementById('breeding-screen').classList.remove('hidden');
    
    renderBreedingPlants();
}

// Chatishtirish uchun o'simliklarni ko'rsatish
function renderBreedingPlants() {
    const breedingPlants = document.getElementById('breeding-plants');
    breedingPlants.innerHTML = '';
    
    const uniquePlants = [...new Set(gameState.inventory)];
    
    uniquePlants.forEach(plant => {
        const plantElement = document.createElement('div');
        plantElement.className = 'breeding-plant';
        plantElement.onclick = () => selectPlantForBreeding(plantElement, plant);
        
        const img = document.createElement('img');
        img.src = crops[plant].image;
        img.alt = crops[plant].name;
        img.style.width = '50px';
        img.style.height = '50px';
        
        const name = document.createElement('div');
        name.textContent = crops[plant].name;
        
        plantElement.appendChild(img);
        plantElement.appendChild(name);
        breedingPlants.appendChild(plantElement);
    });
}

// Chatishtirish uchun o'simlikni tanlash
let selectedPlants = [];
function selectPlantForBreeding(element, plant) {
    if (selectedPlants.includes(plant)) {
        // Tanlovni bekor qilish
        selectedPlants = selectedPlants.filter(p => p !== plant);
        element.classList.remove('selected');
    } else if (selectedPlants.length < 2) {
        // Tanlash
        selectedPlants.push(plant);
        element.classList.add('selected');
    }
    
    if (selectedPlants.length === 2) {
        // Chatishtirishni amalga oshirish
        setTimeout(completeBreeding, 1000);
    }
}

// Chatishtirishni tugatish
function completeBreeding() {
    gameState.money -= 50;
    
    // O'simliklarni inventardan olib tashlash
    selectedPlants.forEach(plant => {
        const index = gameState.inventory.indexOf(plant);
        if (index !== -1) {
            gameState.inventory.splice(index, 1);
        }
    });
    
    // Yangi o'simlik yaratish (oddiy misol)
   // Yangi o'simlik yaratish (oddiy misol)
const newPlant = {
    name: `${crops[selectedPlants[0]].name}-${crops[selectedPlants[1]].name} navi`,
    image: "https://cdn-icons-png.flaticon.com/512/3289/3289718.png",
    growthTime: Math.round((crops[selectedPlants[0]].growthTime + crops[selectedPlants[1]].growthTime) / 2),
    value: Math.round((crops[selectedPlants[0]].value + crops[selectedPlants[1]].value) * 1.5),
    seedCost: Math.round((crops[selectedPlants[0]].seedCost + crops[selectedPlants[1]].seedCost) / 2),
    price: Math.round((crops[selectedPlants[0]].price + crops[selectedPlants[1]].price) * 1.2)
};

// Yangi o'simlikni inventarga qo'shish
gameState.inventory.push(`new_${Date.now()}`);
crops[`new_${Date.now()}`] = newPlant;

// Ekranni yangilash
cancelBreeding();
alert(`Tabriklaymiz! Siz yangi "${newPlant.name}" yaratdingiz!`);
updateResources();
renderFarm();
}

// Chatishtirishni bekor qilish
function cancelBreeding() {
    gameState.breedingMode = false;
    selectedPlants = [];
    document.getElementById('breeding-screen').classList.add('hidden');
    document.getElementById('game-screen').classList.remove('hidden');
    updateResources();
}

function startGame(gender) {
    gameState.player = {
        gender,
        name: gender === 'boy' ? "Fermer Ali" : "Fermer Zaynab"
    };
    
    document.getElementById('player-avatar').src = gender === 'boy' ? 
        "https://cdn-icons-png.flaticon.com/512/4140/4140047.png" : 
        "https://cdn-icons-png.flaticon.com/512/4140/4140048.png";
    
    document.getElementById('player-name').textContent = gameState.player.name;
    
    document.getElementById('character-selection').classList.add('hidden');
    document.getElementById('game-screen').classList.remove('hidden');
    
    renderFarm();
    updateResources();
    
    // Klaviatura boshqaruvini ishga tushiramiz
    setupKeyboardControls();
}


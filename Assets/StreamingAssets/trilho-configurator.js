// Trilho Configurator - JavaScript com Sliders e Mini Mapa
class TrilhoConfigurator {
    constructor() {
        console.log('Inicializando TrilhoConfigurator...');
        this.config = this.getDefaultConfig();
        this.originalConfig = null; // Armazena valores originais do JSON
        this.currentSection = 'general';
        this.zones = [];
        this.originalZones = []; // Armazena zonas originais do JSON
        this.currentTheme = 'dark';
        console.log('Configuração padrão criada:', this.config);
        this.initializeEventListeners();
        
        // Load stored config after DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                // Não carregar automaticamente do localStorage
                // this.loadStoredConfig();
                this.updateLastModified();
                this.showSection('general');
            });
        } else {
            // Não carregar automaticamente do localStorage
            // this.loadStoredConfig();
            this.updateLastModified();
            this.showSection('general');
        }
        
        console.log('TrilhoConfigurator inicializado com sucesso');
    }

    clearOriginalValues() {
        this.originalConfig = null;
        this.originalZones = [];
        console.log('Valores originais limpos');
    }

    getDefaultConfig() {
        return {
            project: {
                name: 'Projeto Trilho',
                description: '',
                version: '1.0'
            },
            trilho: {
                physicalMinCm: 0,
                physicalMaxCm: 600,
                unityMinPosition: 0,
                unityMaxPosition: 8520,
                screenWidthCm: 60,
                movementSensitivity: 0.5
            },
            camera: {
                smoothCameraMovement: true,
                cameraSmoothing: 5
            },
            osc: {
                address: '/unity',
                port: 9000,
                valorNormalizado01: true
            },
            background: {
                enabled: true,
                imageFile: 'Images/background.jpg',
                uploadedFile: null
            },
            zones: [],
            lastModified: new Date().toISOString()
        };
    }

    initializeEventListeners() {
        console.log('Inicializando event listeners...');
        
        // Navigation
        document.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', () => this.switchSection(item));
        });

        // Header buttons
        document.getElementById('save-btn').addEventListener('click', () => this.saveConfig());
        document.getElementById('export-btn').addEventListener('click', () => this.exportConfig());
        document.getElementById('save-to-unity-btn').addEventListener('click', () => this.saveToUnity());
        document.getElementById('load-btn').addEventListener('click', () => this.showLoadModal());
        
        // Debug: botão para limpar localStorage (temporário)
        if (document.getElementById('clear-storage-btn')) {
            document.getElementById('clear-storage-btn').addEventListener('click', () => {
                localStorage.removeItem('trilho-config');
                this.showToast('localStorage limpo!', 'info');
                console.log('localStorage limpo');
            });
        }

        // Form inputs
        this.initializeFormInputs();

        // Reset buttons
        this.initializeResetButtons();

        // Sticky section actions (será inicializado quando uma seção for ativada)
        // this.initializeStickyActions();

        // Teste de sticky (temporário)
        setTimeout(() => {
            this.testSticky();
        }, 2000);

        // Background management
        this.initializeBackgroundEventListeners();

        // Zone management
        document.getElementById('add-zone-btn').addEventListener('click', () => this.addZone());
        
        // Test sticky button
        const testStickyBtn = document.getElementById('test-sticky-btn');
        if (testStickyBtn) {
            testStickyBtn.addEventListener('click', () => {
                console.log('🧪 Botão de teste sticky clicado!');
                this.testSticky();
                
                // Forçar sticky visível para teste
                if (this.currentStickyClone) {
                    console.log('🔧 Forçando sticky visível para teste...');
                    this.currentStickyClone.style.display = 'flex';
                    this.currentStickyClone.style.background = 'red !important';
                    this.currentStickyClone.style.border = '3px solid yellow !important';
                    console.log('🎨 Estilos de teste aplicados');
                } else {
                    console.log('❌ Nenhum clone sticky encontrado para teste');
                }
            });
        }
        

        


        // Modal
        this.initializeModal();
        
        // Mobile menu functionality
        this.initializeMobileMenu();
        
        console.log('Event listeners inicializados com sucesso');
    }

    initializeFormInputs() {
        console.log('Inicializando inputs do formulário...');
        
        // General section
        const generalInputs = ['project-name', 'project-description', 'project-version'];
        generalInputs.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.addEventListener('input', (e) => this.updateConfigValue(e.target.id, e.target.value));
                element.addEventListener('change', (e) => this.updateConfigValue(e.target.id, e.target.value));
            }
        });

        // Trilho section with sliders
        this.initializeSliderInputs();
    }

    initializeSliderInputs() {
        console.log('Inicializando sliders...');
        
        // Trilho sliders
        const trilhoSliders = [
            { sliderId: 'physical-min-cm-slider', inputId: 'physical-min-cm', configPath: 'trilho.physicalMinCm' },
            { sliderId: 'physical-max-cm-slider', inputId: 'physical-max-cm', configPath: 'trilho.physicalMaxCm' },
            { sliderId: 'unity-min-position-slider', inputId: 'unity-min-position', configPath: 'trilho.unityMinPosition' },
            { sliderId: 'unity-max-position-slider', inputId: 'unity-max-position', configPath: 'trilho.unityMaxPosition' },
            { sliderId: 'screen-width-cm-slider', inputId: 'screen-width-cm', configPath: 'trilho.screenWidthCm' },
            { sliderId: 'movement-sensitivity-slider', inputId: 'movement-sensitivity', configPath: 'trilho.movementSensitivity' }
        ];

        trilhoSliders.forEach(({ sliderId, inputId, configPath }) => {
            const slider = document.getElementById(sliderId);
            const input = document.getElementById(inputId);
            
            console.log(`Configurando slider: ${sliderId} -> ${inputId}`, { slider: !!slider, input: !!input });
            
            if (slider && input) {
                // Sync slider with input
                slider.addEventListener('input', (e) => {
                    const value = parseFloat(e.target.value);
                    input.value = value;
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Slider ${sliderId} mudou para: ${value}`);
                });

                // Sync input with slider
                input.addEventListener('input', (e) => {
                    const value = parseFloat(e.target.value);
                    slider.value = value;
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Input ${inputId} mudou para: ${value}`);
                });

                // Update on change
                input.addEventListener('change', (e) => {
                    const value = parseFloat(e.target.value);
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Input ${inputId} confirmou: ${value}`);
                });
            } else {
                console.warn(`Elementos não encontrados para: ${sliderId} ou ${inputId}`);
            }
        });

        // Camera sliders
        const cameraSliders = [
            { sliderId: 'camera-smoothing-slider', inputId: 'camera-smoothing', configPath: 'camera.cameraSmoothing' }
        ];

        cameraSliders.forEach(({ sliderId, inputId, configPath }) => {
            const slider = document.getElementById(sliderId);
            const input = document.getElementById(inputId);
            
            console.log(`Configurando câmera: ${sliderId} -> ${inputId}`, { slider: !!slider, input: !!input });
            
            if (slider && input) {
                slider.addEventListener('input', (e) => {
                    const value = parseFloat(e.target.value);
                    input.value = value;
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Slider câmera mudou para: ${value}`);
                });

                input.addEventListener('input', (e) => {
                    const value = parseFloat(e.target.value);
                    slider.value = value;
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Input câmera mudou para: ${value}`);
                });

                input.addEventListener('change', (e) => {
                    const value = parseFloat(e.target.value);
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Input câmera confirmou: ${value}`);
                });
            } else {
                console.warn(`Elementos de câmera não encontrados para: ${sliderId} ou ${inputId}`);
            }
        });

        // OSC fields (sem sliders, apenas inputs)
        const oscFields = [
            { inputId: 'osc-address', configPath: 'osc.address' },
            { inputId: 'osc-port', configPath: 'osc.port' }
        ];

        oscFields.forEach(({ inputId, configPath }) => {
            const input = document.getElementById(inputId);
            
            console.log(`Configurando OSC: ${inputId}`, { input: !!input });
            
            if (input) {
                input.addEventListener('input', (e) => {
                    const value = inputId === 'osc-port' ? parseInt(e.target.value) : e.target.value;
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Input OSC ${inputId} mudou para: ${value}`);
                });

                input.addEventListener('change', (e) => {
                    const value = inputId === 'osc-port' ? parseInt(e.target.value) : e.target.value;
                    this.updateConfigValueByPath(configPath, value);
                    console.log(`Input OSC ${inputId} confirmou: ${value}`);
                });
            } else {
                console.warn(`Elemento OSC não encontrado para: ${inputId}`);
            }
        });
    }

    updateConfigValue(id, value) {
        console.log(`Atualizando configuração: ${id} = ${value}`);
        
        const pathParts = id.split('-');
        if (pathParts.length >= 2) {
            const section = pathParts[0];
            const field = pathParts.slice(1).join('-');
            
            if (this.config[section]) {
                this.config[section][field] = value;
                this.updateLastModified();
                console.log(`Configuração atualizada: ${section}.${field} = ${value}`);
            }
        }
    }

    updateConfigValueByPath(path, value) {
        console.log(`Atualizando configuração por caminho: ${path} = ${value}`);
        
        const pathParts = path.split('.');
        let current = this.config;
        
        for (let i = 0; i < pathParts.length - 1; i++) {
            if (!current[pathParts[i]]) {
                current[pathParts[i]] = {};
            }
            current = current[pathParts[i]];
        }
        
        current[pathParts[pathParts.length - 1]] = value;
        this.updateLastModified();
        console.log(`Configuração atualizada por caminho: ${path} = ${value}`);
    }



    switchSection(clickedItem) {
        document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));
        clickedItem.classList.add('active');
        
        const sectionName = clickedItem.getAttribute('data-section');
        this.showSection(sectionName);
    }

    showSection(sectionName) {
        // Ocultar todas as seções
        document.querySelectorAll('.content-section').forEach(section => {
            section.classList.remove('active');
        });
        
        // Mostrar a seção selecionada
        const targetSection = document.getElementById(`${sectionName}-section`);
        if (targetSection) {
            targetSection.classList.add('active');
            this.currentSection = sectionName;
            console.log(`Seção exibida: ${sectionName}`);
            
                    // Se for a seção zones, atualizar o minimapa
        if (sectionName === 'zones') {
            console.log('Seção zones ativada, atualizando minimapa...');
            
            // Debug completo do DOM
            console.log('=== DEBUG DOM ===');
            console.log('Document readyState:', document.readyState);
            console.log('Body children count:', document.body.children.length);
            
            // Verificar se o elemento existe
            const minimap = document.getElementById('trilho-minimap');
            console.log('Elemento trilho-minimap existe?', !!minimap);
            console.log('Elemento encontrado:', minimap);
            
            // Verificar se a seção zones está visível
            const zonesSection = document.getElementById('zones-section');
            console.log('Seção zones encontrada:', zonesSection);
            if (zonesSection) {
                console.log('Seção zones display:', getComputedStyle(zonesSection).display);
                console.log('Seção zones visibility:', getComputedStyle(zonesSection).visibility);
            }
            
            // Verificar se o container do minimapa está visível
            const minimapContainer = document.querySelector('.trilho-minimap-container');
            console.log('Container minimapa encontrado:', minimapContainer);
            if (minimapContainer) {
                console.log('Container minimapa display:', getComputedStyle(minimapContainer).display);
                console.log('Container minimapa visibility:', getComputedStyle(minimapContainer).visibility);
            }
            
            if (minimap) {
                console.log('Minimap encontrado imediatamente, atualizando...');
                this.updateTrilhoMinimap();
            } else {
                console.log('Minimap não encontrado, aguardando renderização...');
                console.log('Zonas disponíveis:', this.zones.length);
                
                // Aguardar mais tempo para o DOM renderizar completamente
                setTimeout(() => this.updateTrilhoMinimap(), 500);
            }
        }
        
        // Recriar sticky para a nova seção
        setTimeout(() => {
            this.setupJavaScriptSticky();
        }, 100);
        } else {
            console.error(`Seção ${sectionName} não encontrada`);
        }
    }

    addZone() {
        const newZone = {
            id: Date.now().toString(),
            name: `Zona ${this.zones.length + 1}`,
            type: 0, // 0: Imagem, 1: Vídeo, 2: Texto, 3: Aplicação
            positionCm: 0,
            widthCm: 30,
            heightCm: 20,
            imageSettings: {
                imageFile: '',
                uploadedFile: null
            },
            videoSettings: {
                videoFile: '',
                uploadedFile: null,
                loop: true
            },
            textSettings: {
                text: 'Texto da zona',
                fontSize: 24,
                textColor: [0, 0, 0, 1]
            }
        };

        this.zones.push(newZone);
        this.renderZone(newZone, this.zones.length - 1);
        this.updateLastModified();
        console.log('Nova zona adicionada:', newZone);
    }

    renderZone(zone, index) {
        console.log('Renderizando zona:', zone.name);
        console.log('Dados da zona:', {
            name: zone.name,
            type: zone.type,
            positionCm: zone.positionCm,
            widthCm: zone.widthCm,
            heightCm: zone.heightCm,
            textSettings: zone.textSettings,
            imageSettings: zone.imageSettings,
            videoSettings: zone.videoSettings
        });
        
        const zonesContainer = document.getElementById('zones-container');
        if (!zonesContainer) {
            console.error('Container de zonas não encontrado');
            return;
        }

        const zoneElement = document.createElement('div');
        zoneElement.className = 'zone-item';
        zoneElement.setAttribute('data-zone-id', zone.id);

        zoneElement.innerHTML = `
            <div class="zone-header">
                <div class="zone-header-info">
                    <h4 class="zone-title" contenteditable="true" data-placeholder="Clique para editar">${zone.name}</h4>
                    <div class="zone-position-info">
                        <span class="zone-order">#${(index || 0) + 1}</span>
                        <span class="zone-position">${zone.positionCm || 0}cm</span>
                        <span class="zone-type-badge" style="background-color: ${this.getZoneColor(zone.type)}; color: white;">${this.getZoneTypeName(zone.type)}</span>
                    </div>
                </div>
                <div class="zone-actions">
                    <button class="btn btn-sm btn-secondary toggle-zone-btn" title="Expandir/Recolher zona">
                        <i class="fas fa-chevron-down"></i>
                    </button>
                    <button class="btn btn-sm btn-danger delete-zone-btn" title="Excluir zona">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
            <div class="zone-content" style="display: none;">
                <div class="zone-settings">
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Tipo de Conteúdo</label>
                        <select class="zone-type-input form-select">
                            <option value="0" ${zone.type === 0 ? 'selected' : ''}>Imagem</option>
                            <option value="1" ${zone.type === 1 ? 'selected' : ''}>Vídeo</option>
                            <option value="2" ${zone.type === 2 ? 'selected' : ''}>Texto</option>
                            <option value="3" ${zone.type === 3 ? 'selected' : ''}>Aplicação</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Posição (cm)</label>
                        <div class="slider-container">
                            <input type="range" class="zone-position-slider" min="0" max="600" step="1" value="${zone.positionCm || 0}">
                            <input type="number" class="zone-position-input form-input" value="${zone.positionCm || 0}" step="0.1">
                        </div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label class="form-label">Largura (px)</label>
                        <div class="slider-container">
                            <input type="range" class="zone-width-slider" min="100" max="2000" step="10" value="${zone.widthCm || 800}">
                            <input type="number" class="zone-width-input form-input" value="${zone.widthCm || 800}" step="0.1">
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Altura (px)</label>
                        <div class="slider-container">
                            <input type="range" class="zone-height-slider" min="100" max="1200" step="10" value="${zone.heightCm || 600}">
                            <input type="number" class="zone-height-input form-input" value="${zone.heightCm || 600}" step="0.1">
                        </div>
                    </div>
                </div>
                
                <div class="zone-type-settings">
                    <div class="zone-image-settings" style="display: ${zone.type === 0 ? 'block' : 'none'};">
                        <div class="form-group">
                            <label class="form-label">Arquivo de Imagem</label>
                            <input type="file" class="zone-image-file" accept="image/*">
                            <input type="text" class="zone-image-path form-input" placeholder="Caminho da imagem" value="${zone.imageSettings?.imageFile || ''}">
                        </div>
                    </div>
                    
                    <div class="zone-video-settings" style="display: ${zone.type === 1 ? 'block' : 'none'};">
                        <div class="form-group">
                            <label class="form-label">Arquivo de Vídeo</label>
                            <input type="file" class="zone-video-file" accept="video/*">
                            <input type="text" class="zone-video-path form-input" placeholder="Caminho do vídeo" value="${zone.videoSettings?.videoFile || ''}">
                        </div>
                        <div class="form-group">
                            <label>
                                <input type="checkbox" class="zone-video-loop form-checkbox" ${zone.videoSettings?.loop ? 'checked' : ''}>
                                Loop
                            </label>
                        </div>
                    </div>
                    
                    <div class="zone-text-settings" style="display: ${zone.type === 2 ? 'block' : 'none'};">
                        <div class="form-group">
                            <label class="form-label">Texto</label>
                            <textarea class="zone-text-content form-textarea" placeholder="Digite o texto da zona">${zone.textSettings?.text || 'Texto da zona'}</textarea>
                        </div>
                        <div class="form-row">
                            <div class="form-group">
                                <label class="form-label">Tamanho da Fonte</label>
                                <div class="slider-container">
                                    <input type="range" class="zone-fontsize-slider" min="8" max="72" value="${zone.textSettings?.fontSize || 24}">
                                    <input type="number" class="zone-fontsize-input form-input" value="${zone.textSettings?.fontSize || 24}" min="8" max="72">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="form-label">Cor do Texto</label>
                                <input type="color" class="zone-text-color" value="${this.arrayToHexColor(zone.textSettings?.textColor || [0, 0, 0, 1])}">
                            </div>
                        </div>
                    </div>
                </div>
                </div>
            </div>
        `;

        console.log('HTML da zona gerado:', zoneElement.innerHTML);
        zonesContainer.appendChild(zoneElement);
        console.log('Zona adicionada ao container');
        
        this.addZoneEventListeners(zoneElement, zone);
        console.log('Event listeners adicionados à zona');
        
        // Mostrar configurações específicas baseado no tipo atual
        this.showZoneTypeSettings(zoneElement, zone.type);
        console.log('Configurações específicas configuradas para tipo:', zone.type);
    }

    addZoneEventListeners(zoneElement, zone) {
        // Zone name editing
        const zoneTitle = zoneElement.querySelector('.zone-title');
        if (zoneTitle) {
            zoneTitle.addEventListener('blur', () => {
                zone.name = zoneTitle.textContent.trim() || 'Zona sem nome';
                this.updateLastModified();
            });
            
            zoneTitle.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    zoneTitle.blur();
                }
            });
        }

        // Zone type change
        const zoneTypeInput = zoneElement.querySelector('.zone-type-input');
        if (zoneTypeInput) {
            zoneTypeInput.addEventListener('change', (e) => {
                zone.type = parseInt(e.target.value);
                this.showZoneTypeSettings(zoneElement, zone.type);
                this.updateLastModified();
            });
        }

        // Position and dimensions with sliders
        const positionSlider = zoneElement.querySelector('.zone-position-slider');
        const positionInput = zoneElement.querySelector('.zone-position-input');
        if (positionSlider && positionInput) {
            positionSlider.addEventListener('input', (e) => {
                const value = parseInt(e.target.value);
                zone.positionCm = value;
                positionInput.value = value;
                this.updateLastModified();
            });
            positionInput.addEventListener('input', (e) => {
                const value = parseFloat(e.target.value) || 0;
                zone.positionCm = value;
                positionSlider.value = value;
                this.updateLastModified();
            });
        }

        const widthSlider = zoneElement.querySelector('.zone-width-slider');
        const widthInput = zoneElement.querySelector('.zone-width-input');
        if (widthSlider && widthInput) {
            widthSlider.addEventListener('input', (e) => {
                const value = parseInt(e.target.value);
                zone.widthCm = value;
                widthInput.value = value;
                this.updateLastModified();
            });
            widthInput.addEventListener('input', (e) => {
                const value = parseFloat(e.target.value) || 800;
                zone.widthCm = value;
                widthSlider.value = value;
                this.updateLastModified();
            });
        }

        const heightSlider = zoneElement.querySelector('.zone-height-slider');
        const heightInput = zoneElement.querySelector('.zone-height-input');
        if (heightSlider && heightInput) {
            heightSlider.addEventListener('input', (e) => {
                const value = parseInt(e.target.value);
                zone.heightCm = value;
                heightInput.value = value;
                this.updateLastModified();
            });
            heightInput.addEventListener('input', (e) => {
                const value = parseFloat(e.target.value) || 600;
                zone.heightCm = value;
                heightSlider.value = value;
                this.updateLastModified();
            });
        }

        // Image settings
        const imageFileInput = zoneElement.querySelector('.zone-image-file');
        const imagePathInput = zoneElement.querySelector('.zone-image-path');
        if (imageFileInput && imagePathInput) {
            imageFileInput.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    zone.imageSettings.uploadedFile = file;
                    zone.imageSettings.imageFile = file.name;
                    imagePathInput.value = file.name;
                    this.updateLastModified();
                }
            });

            imagePathInput.addEventListener('input', (e) => {
                zone.imageSettings.imageFile = e.target.value;
                this.updateLastModified();
            });
        }

        // Video settings
        const videoFileInput = zoneElement.querySelector('.zone-video-file');
        const videoPathInput = zoneElement.querySelector('.zone-video-path');
        const videoLoopInput = zoneElement.querySelector('.zone-video-loop');
        if (videoFileInput && videoPathInput && videoLoopInput) {
            videoFileInput.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    zone.videoSettings.uploadedFile = file;
                    zone.videoSettings.videoFile = file.name;
                    videoPathInput.value = file.name;
                    this.updateLastModified();
                }
            });

            videoPathInput.addEventListener('input', (e) => {
                zone.videoSettings.videoFile = e.target.value;
                this.updateLastModified();
            });

            videoLoopInput.addEventListener('change', (e) => {
                zone.videoSettings.loop = e.target.checked;
                this.updateLastModified();
            });
        }

        // Text settings
        this.addTextSettingsEventListeners(zoneElement, zone);

        // Toggle zone expand/collapse
        const toggleBtn = zoneElement.querySelector('.toggle-zone-btn');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.toggleZone(zoneElement);
            });
        }

        // Delete zone
        const deleteBtn = zoneElement.querySelector('.delete-zone-btn');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', () => this.deleteZone(zone.id));
        }
    }

    deleteZone(zoneId) {
        // Confirmar antes de deletar
        const zone = this.zones.find(z => z.id === zoneId);
        if (!zone) return;
        
        const confirmMessage = `Tem certeza que deseja remover a zona "${zone.name}"?`;
        if (!confirm(confirmMessage)) {
            return;
        }
        
        const zoneIndex = this.zones.findIndex(z => z.id === zoneId);
        if (zoneIndex !== -1) {
            // Salvar o estado de retraído/expandido das outras zonas
            const zonesState = {};
            this.zones.forEach((z, index) => {
                if (index !== zoneIndex) {
                    const zoneEl = document.querySelector(`[data-zone-id="${z.id}"]`);
                    if (zoneEl) {
                        const contentEl = zoneEl.querySelector('.zone-content');
                        const toggleBtn = zoneEl.querySelector('.toggle-zone-btn i');
                        zonesState[z.id] = {
                            expanded: contentEl ? contentEl.style.display !== 'none' : false,
                            rotation: toggleBtn ? toggleBtn.style.transform : 'rotate(0deg)'
                        };
                    }
                }
            });
            
            // Remover a zona
            this.zones.splice(zoneIndex, 1);
            const zoneElement = document.querySelector(`[data-zone-id="${zoneId}"]`);
            if (zoneElement) {
                zoneElement.remove();
            }
            this.updateLastModified();
            console.log(`Zona ${zoneId} removida`);
            
            // Re-renderizar zonas mantendo o estado anterior
            this.renderZones();
            
            // Restaurar o estado de retraído/expandido
            setTimeout(() => {
                this.zones.forEach(z => {
                    if (zonesState[z.id] !== undefined) {
                        const zoneEl = document.querySelector(`[data-zone-id="${z.id}"]`);
                        if (zoneEl) {
                            const contentEl = zoneEl.querySelector('.zone-content');
                            const toggleBtn = zoneEl.querySelector('.toggle-zone-btn i');
                            if (contentEl && toggleBtn) {
                                // Restaurar estado exato
                                contentEl.style.display = zonesState[z.id].expanded ? 'block' : 'none';
                                toggleBtn.style.transform = zonesState[z.id].rotation;
                            }
                        }
                    }
                });
            }, 100);
            
            // Atualizar minimapa
            this.updateTrilhoMinimap();
            
            // Mostrar toast de confirmação
            this.showToast('Zona removida com sucesso!', 'success');
        }
    }

    saveZone(zone) {
        const zoneIndex = this.zones.findIndex(z => z.id === zone.id);
        if (zoneIndex !== -1) {
            this.zones[zoneIndex] = { ...zone };
            this.updateLastModified();
            console.log(`Zona ${zone.id} salva`);
        }
    }

    updateTrilhoMinimap() {
        console.log('=== ATUALIZANDO MINIMAPA ===');
        console.log('Configuração atual:', this.config);
        console.log('Zonas disponíveis:', this.zones);
        
        // Verificar se há zonas para mostrar
        if (!this.zones || this.zones.length === 0) {
            console.log('Nenhuma zona para mostrar no minimapa');
            return;
        }
        
        // Verificar se já está sendo executado para evitar loops
        if (this._updatingMinimap) {
            console.log('Update do minimapa já em andamento, ignorando...');
            return;
        }
        
        this._updatingMinimap = true;
        
        // Verificar se o DOM está pronto
        const minimap = document.getElementById('trilho-minimap');
        console.log('Minimap encontrado:', minimap);
        
        if (!minimap) {
            console.error('Elemento trilho-minimap não encontrado!');
            console.log('Tentando novamente em 200ms...');
            
            // Debug adicional
            console.log('=== DEBUG MINIMAPA ===');
            console.log('Procurando por trilho-minimap...');
            
            // Verificar se o container existe
            const container = document.querySelector('.trilho-minimap-container');
            console.log('Container encontrado:', container);
            
            if (container) {
                console.log('Container HTML:', container.innerHTML);
                console.log('Container children:', container.children);
                
                // Procurar por qualquer elemento com trilho-minimap
                const allElements = document.querySelectorAll('*');
                const minimapElements = Array.from(allElements).filter(el => 
                    el.id === 'trilho-minimap' || 
                    el.className.includes('trilho-minimap')
                );
                console.log('Elementos com trilho-minimap encontrados:', minimapElements);
            }
            
            this._updatingMinimap = false;
            
            // Tentar novamente se o elemento não for encontrado
            setTimeout(() => this.updateTrilhoMinimap(), 200);
            return;
        }

        // Limpar bullets existentes
        const zonesContainer = minimap.querySelector('.trilho-zones');
        console.log('Container trilho-zones encontrado:', zonesContainer);
        
        if (!zonesContainer) {
            console.error('Container trilho-zones não encontrado!');
            this._updatingMinimap = false;
            return;
        }

        // Limpar conteúdo anterior
        zonesContainer.innerHTML = '';
        console.log('Container limpo, criando bullets...');

        // Criar bullets para cada zona
        this.zones.forEach((zone, index) => {
            console.log(`Criando bullet para zona ${index}:`, zone);
            console.log(`Nome: ${zone.name}, Posição: ${zone.positionCm}cm, Tipo: ${zone.type}`);
            
            const marker = document.createElement('div');
            marker.className = 'trilho-zone-marker';
            
            // Verificar se a configuração tem physicalMaxCm
            if (!this.config.trilho || !this.config.trilho.physicalMaxCm) {
                console.error('Configuração trilho.physicalMaxCm não encontrada!');
                console.log('Config trilho:', this.config.trilho);
                this._updatingMinimap = false;
                return;
            }
            
            // Calcular posição
            const positionPercent = (zone.positionCm / this.config.trilho.physicalMaxCm) * 100;
            marker.style.left = `${positionPercent}%`;
            console.log(`Posição calculada: ${zone.positionCm}cm / ${this.config.trilho.physicalMaxCm}cm = ${positionPercent}%`);
            
            // Aplicar cor baseada no tipo de conteúdo
            const zoneColor = this.getZoneColor(zone.type);
            marker.style.background = zoneColor;
            marker.style.borderColor = this.getZoneTypeColor(zone.type);
            
            // Adicionar informações visuais ao bullet
            marker.innerHTML = `
                <div class="marker-tooltip">
                    <div class="marker-name">${zone.name}</div>
                    <div class="marker-position">${zone.positionCm}cm</div>
                    <div class="marker-type" style="color: ${zoneColor}">${this.getZoneTypeName(zone.type)}</div>
                </div>
            `;
            
            // Adicionar evento de clique para focar na zona
            marker.addEventListener('click', () => {
                this.focusZone(zone.id);
            });
            
            // Adicionar evento de hover para mostrar tooltip
            marker.addEventListener('mouseenter', () => {
                marker.classList.add('hovered');
            });
            
            marker.addEventListener('mouseleave', () => {
                marker.classList.remove('hovered');
            });
            
            zonesContainer.appendChild(marker);
            console.log(`Bullet criado e adicionado para zona: ${zone.name}`);
        });
        
        console.log(`${this.zones.length} bullets criados no minimapa`);
        console.log('=== MINIMAPA ATUALIZADO ===');
        this._updatingMinimap = false;
    }

    getZoneTypeName(type) {
        const types = ['Imagem', 'Vídeo', 'Texto', 'Aplicação'];
        return types[type] || 'Desconhecido';
    }

    focusZone(zoneId) {
        // Focar na zona específica
        console.log('Focando na zona:', zoneId);
        
        // Encontrar o elemento da zona
        const zoneElement = document.querySelector(`[data-zone-id="${zoneId}"]`);
        if (!zoneElement) {
            console.error('Zona não encontrada:', zoneId);
            return;
        }
        
        // Expandir a zona se estiver colapsada
        const content = zoneElement.querySelector('.zone-content');
        if (content.classList.contains('collapsed')) {
            this.toggleZone(zoneElement);
        }
        
        // Rolar para a zona
        zoneElement.scrollIntoView({ 
            behavior: 'smooth', 
            block: 'center' 
        });
        
        // Adicionar destaque temporário
        zoneElement.classList.add('highlighted');
        setTimeout(() => {
            zoneElement.classList.remove('highlighted');
        }, 2000);
        
        // Mostrar seção de zonas se não estiver visível
        this.showSection('zones');
    }

    toggleZone(zoneElement) {
        const content = zoneElement.querySelector('.zone-content');
        const toggleBtn = zoneElement.querySelector('.toggle-zone-btn i');
        
        if (content.style.display === 'none') {
            // Expandir zona
            content.style.display = 'block';
            toggleBtn.style.transform = 'rotate(180deg)';
        } else {
            // Retrair zona
            content.style.display = 'none';
            toggleBtn.style.transform = 'rotate(0deg)';
        }
    }

    getZoneColor(type) {
        const colors = {
            0: '#4CAF50',  // Imagem - Verde
            1: '#2196F3',  // Vídeo - Azul
            2: '#FF9800',  // Texto - Laranja
            3: '#9C27B0'   // Aplicação - Roxo
        };
        return colors[type] || '#666';
    }
    
    getZoneTypeColor(type) {
        const colors = {
            0: '#2E7D32',  // Imagem - Verde escuro
            1: '#1565C0',  // Vídeo - Azul escuro
            2: '#E65100',  // Texto - Laranja escuro
            3: '#6A1B9A'   // Aplicação - Roxo escuro
        };
        return colors[type] || '#424242';
    }

    initializeBackgroundEventListeners() {
        const backgroundEnabled = document.getElementById('background-enabled');
        const backgroundImageFile = document.getElementById('background-image-file');
        const backgroundUpload = document.getElementById('background-upload');

        if (backgroundEnabled) {
            backgroundEnabled.addEventListener('change', (e) => {
                this.config.background.enabled = e.target.checked;
                this.updateLastModified();
            });
        }

        if (backgroundImageFile) {
            backgroundImageFile.addEventListener('input', (e) => {
                this.config.background.imageFile = e.target.value;
                this.updateLastModified();
            });
        }

        if (backgroundUpload) {
            backgroundUpload.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    this.config.background.uploadedFile = file;
                    this.config.background.imageFile = file.name;
                    document.getElementById('background-image-file').value = file.name;
                    this.updateLastModified();
                }
            });
        }
    }

    initializeMobileMenu() {
        const mobileMenuToggle = document.getElementById('mobile-menu-toggle');
        const sidebar = document.querySelector('.sidebar');
        const sidebarOverlay = document.getElementById('sidebar-overlay');
        
        if (mobileMenuToggle && sidebar && sidebarOverlay) {
            // Toggle sidebar
            mobileMenuToggle.addEventListener('click', () => {
                sidebar.classList.toggle('active');
                sidebarOverlay.classList.toggle('active');
                document.body.style.overflow = sidebar.classList.contains('active') ? 'hidden' : '';
            });
            
            // Fechar sidebar ao clicar no overlay
            sidebarOverlay.addEventListener('click', () => {
                sidebar.classList.remove('active');
                sidebarOverlay.classList.remove('active');
                document.body.style.overflow = '';
            });
            
            // Fechar sidebar ao clicar em um item do menu
            const navItems = sidebar.querySelectorAll('.nav-item');
            navItems.forEach(item => {
                item.addEventListener('click', () => {
                    if (window.innerWidth <= 768) {
                        sidebar.classList.remove('active');
                        sidebarOverlay.classList.remove('active');
                        document.body.style.overflow = '';
                    }
                });
            });
            
            // Fechar sidebar ao redimensionar a tela
            window.addEventListener('resize', () => {
                if (window.innerWidth > 768) {
                    sidebar.classList.remove('active');
                    sidebarOverlay.classList.remove('active');
                    document.body.style.overflow = '';
                }
            });
        }
    }

    initializeModal() {
        console.log('Inicializando modal...');
        const modal = document.getElementById('load-modal');
        console.log('Modal encontrado:', modal);
        
        if (!modal) {
            console.error('Modal não encontrado!');
            return;
        }
        
        const closeBtn = modal.querySelector('#close-modal');
        const fileInput = modal.querySelector('#load-json-file');
        const loadBtn = modal.querySelector('#load-file-btn');
        const cancelBtn = modal.querySelector('#cancel-load-btn');
        
        console.log('Close button:', closeBtn);
        console.log('File input:', fileInput);
        console.log('Load button:', loadBtn);
        console.log('Cancel button:', cancelBtn);

        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                console.log('Close button clicado');
                this.hideLoadModal();
            });
        }

        if (cancelBtn) {
            cancelBtn.addEventListener('click', () => {
                console.log('Cancel button clicado');
                this.hideLoadModal();
            });
        }

        if (loadBtn) {
            loadBtn.addEventListener('click', () => {
                console.log('Load button clicado');
                if (fileInput && fileInput.files[0]) {
                    this.handleFileSelection(fileInput.files[0]);
                } else {
                    this.showToast('Selecione um arquivo primeiro', 'warning');
                }
            });
        }

        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    console.log('Modal clicado (fechando)');
                    this.hideLoadModal();
                }
            });
        }

        if (fileInput) {
            fileInput.addEventListener('change', (e) => {
                console.log('Arquivo selecionado no input');
                // Não carregar automaticamente, aguardar clique no botão Carregar
            });
        }
        
        console.log('Modal inicializado com sucesso');
    }

    handleFileSelection(file) {
        console.log('Arquivo selecionado:', file.name, 'Tamanho:', file.size);
        
        const reader = new FileReader();
        reader.onload = (e) => {
            console.log('Arquivo lido com sucesso, tamanho do conteúdo:', e.target.result.length);
            try {
                const jsonConfig = JSON.parse(e.target.result);
                console.log('JSON parseado com sucesso:', jsonConfig);
                this.loadConfigFromJSON(jsonConfig);
                this.hideLoadModal();
            } catch (error) {
                console.error('Erro ao carregar arquivo:', error);
                this.showToast('Erro ao carregar arquivo de configuração: ' + error.message, 'error');
            }
        };
        
        reader.onerror = (error) => {
            console.error('Erro ao ler arquivo:', error);
            this.showToast('Erro ao ler arquivo', 'error');
        };
        
        reader.readAsText(file);
    }

    loadConfigFromJSON(jsonConfig) {
        console.log('=== CARREGANDO CONFIGURAÇÃO ===');
        console.log('JSON recebido:', jsonConfig);
        console.log('Chaves disponíveis:', Object.keys(jsonConfig));
        
        try {
            // Reset current config
            this.config = this.getDefaultConfig();
            console.log('Configuração padrão resetada:', this.config);
            
            // Load project info
            if (jsonConfig.project) {
                this.config.project = { ...this.config.project, ...jsonConfig.project };
                console.log('Configuração do projeto carregada:', this.config.project);
            } else {
                console.log('Nenhuma configuração de projeto encontrada no JSON');
            }
            
            // Load trilho settings
            if (jsonConfig.trilho) {
                this.config.trilho = { ...this.config.trilho, ...jsonConfig.trilho };
                console.log('Configuração do trilho carregada:', this.config.trilho);
            } else {
                console.log('Nenhuma configuração de trilho encontrada no JSON');
            }
            
            // Mapear campos do formato antigo para o novo
            console.log('=== MAPEANDO CAMPOS ANTIGOS ===');
            if (jsonConfig.physicalMaxCm !== undefined) {
                this.config.trilho.physicalMaxCm = jsonConfig.physicalMaxCm;
                console.log('physicalMaxCm mapeado:', this.config.trilho.physicalMaxCm);
            }
            
            if (jsonConfig.unityMinPosition !== undefined) {
                this.config.trilho.unityMinPosition = jsonConfig.unityMinPosition;
                console.log('unityMinPosition mapeado:', this.config.trilho.unityMinPosition);
            }
            
            if (jsonConfig.unityMaxPosition !== undefined) {
                this.config.trilho.unityMaxPosition = jsonConfig.unityMaxPosition;
                console.log('unityMaxPosition mapeado:', this.config.trilho.unityMaxPosition);
            }
            
            if (jsonConfig.screenWidthInCm !== undefined) {
                this.config.trilho.screenWidthCm = jsonConfig.screenWidthInCm;
                console.log('screenWidthInCm mapeado:', this.config.trilho.screenWidthCm);
            }
            
            if (jsonConfig.movementSensitivity !== undefined) {
                this.config.trilho.movementSensitivity = jsonConfig.movementSensitivity;
                console.log('movementSensitivity mapeado:', this.config.trilho.movementSensitivity);
            }
            
            // Load camera settings
            if (jsonConfig.camera) {
                this.config.camera = { ...this.config.camera, ...jsonConfig.camera };
                console.log('Configuração da câmera carregada:', this.config.camera);
            } else {
                console.log('Nenhuma configuração de câmera encontrada no JSON');
            }
            
            // Load OSC settings
            if (jsonConfig.osc) {
                this.config.osc = { ...this.config.osc, ...jsonConfig.osc };
                console.log('Configuração OSC carregada:', this.config.osc);
            } else {
                console.log('Nenhuma configuração OSC encontrada no JSON');
            }
            
            // Load background settings
            if (jsonConfig.background) {
                this.config.background = { ...this.config.background, ...jsonConfig.background };
                console.log('Configuração do background carregada:', this.config.background);
            } else {
                console.log('Nenhuma configuração de background encontrada no JSON');
            }
            
            // Load zones
            console.log('=== CARREGANDO ZONAS ===');
            console.log('jsonConfig.zones existe?', !!jsonConfig.zones);
            console.log('jsonConfig.zones é array?', Array.isArray(jsonConfig.zones));
            console.log('jsonConfig.zones:', jsonConfig.zones);
            console.log('jsonConfig.contentZones existe?', !!jsonConfig.contentZones);
            console.log('jsonConfig.contentZones é array?', Array.isArray(jsonConfig.contentZones));
            console.log('jsonConfig.contentZones:', jsonConfig.contentZones);
            
            // Tentar carregar de ambas as chaves possíveis
            let zonesToLoad = null;
            if (jsonConfig.contentZones && Array.isArray(jsonConfig.contentZones)) {
                zonesToLoad = jsonConfig.contentZones;
                console.log('Usando contentZones');
            } else if (jsonConfig.zones && Array.isArray(jsonConfig.zones)) {
                zonesToLoad = jsonConfig.zones;
                console.log('Usando zones');
            }
            
            if (zonesToLoad) {
                console.log('Carregando zonas:', zonesToLoad.length);
                console.log('Primeira zona do JSON:', zonesToLoad[0]);
                console.log('Estrutura completa do JSON:', jsonConfig);
                
                this.zones = zonesToLoad.map(zone => {
                    console.log('Processando zona:', zone.name);
                    console.log('Zona original:', zone);
                    
                    // Determinar o tipo da zona
                    const zoneType = zone.contentType !== undefined ? zone.contentType : zone.type;
                    console.log('Tipo da zona:', zoneType);
                    
                    // Determinar o caminho do conteúdo
                    const contentPath = zone.contentPath || zone.imageFile || zone.videoFile || '';
                    console.log('Caminho do conteúdo:', contentPath);
                    
                    const processedZone = {
                        ...zone,
                        id: zone.id || Date.now().toString(),
                        type: zoneType,
                        positionCm: zone.positionCm || 0,
                        widthCm: zone.imageSettings?.size?.[0] || zone.widthCm || 800,
                        heightCm: zone.imageSettings?.size?.[1] || zone.heightCm || 600,
                        imageSettings: { 
                            imageFile: contentPath,
                            uploadedFile: null,
                            ...zone.imageSettings
                        },
                        videoSettings: { 
                            videoFile: contentPath,
                            uploadedFile: null,
                            loop: zone.videoSettings?.loop !== false,
                            ...zone.videoSettings
                        },
                        textSettings: { 
                            text: zone.textSettings?.text || 'Texto da zona',
                            fontSize: zone.textSettings?.fontSize || zone.textSettings?.textSize || 24,
                            textColor: zone.textSettings?.textColor || [0, 0, 0, 1],
                            ...zone.textSettings
                        }
                    };
                    
                    console.log('Zona processada:', processedZone);
                    return processedZone;
                });
                
                console.log('Todas as zonas processadas:', this.zones);
                console.log('Contagem final de zonas:', this.zones.length);
            } else {
                console.log('Nenhuma zona encontrada no JSON');
                console.log('Estrutura do JSON:', Object.keys(jsonConfig));
                console.log('JSON completo:', jsonConfig);
                this.zones = [];
            }
            
            console.log('=== ATUALIZANDO INTERFACE ===');
            console.log('Configuração final antes de popular campos:', this.config);
            
            // ARMazenar valores originais para reset
            this.originalConfig = JSON.parse(JSON.stringify(this.config));
            this.originalZones = JSON.parse(JSON.stringify(this.zones));
            console.log('=== VALORES ORIGINAIS ARMAZENADOS ===');
            console.log('Configuração original:', this.originalConfig);
            console.log('Zonas originais:', this.originalZones);
            
            // Verificar estrutura das zonas originais
            console.log('=== ESTRUTURA DAS ZONAS ORIGINAIS ===');
            this.originalZones.forEach((zone, index) => {
                console.log(`Zona original ${index + 1}:`, {
                    name: zone.name,
                    widthCm: zone.widthCm,
                    heightCm: zone.heightCm,
                    positionCm: zone.positionCm,
                    type: zone.type,
                    imageSettings: zone.imageSettings,
                    videoSettings: zone.videoSettings,
                    textSettings: zone.textSettings
                });
            });
            
            // Update UI
            this.populateFormFields();
            console.log('Campos do formulário populados');
            
            this.renderZones();
            console.log('Zonas renderizadas');
            
            console.log('=== CONFIGURAÇÃO CARREGADA COM SUCESSO ===');
            
            // Mostrar toast de sucesso
            this.showToast('Configuração carregada com sucesso!', 'success');
            
        } catch (error) {
            console.error('Erro ao carregar configuração:', error);
            this.showToast('Erro ao carregar configuração: ' + error.message, 'error');
        }
    }

    convertV3ToConfiguratorFormat(v3Config) {
        const config = this.getDefaultConfig();
        
        if (v3Config.project) {
            config.project = { ...config.project, ...v3Config.project };
        }
        
        if (v3Config.trilho) {
            config.trilho = { ...config.trilho, ...v3Config.trilho };
        }
        
        if (v3Config.camera) {
            config.camera = { ...config.camera, ...v3Config.camera };
        }
        
        if (v3Config.osc) {
            config.osc = { ...config.osc, ...v3Config.osc };
        }
        
        if (v3Config.background) {
            config.background = { ...config.background, ...v3Config.background };
        }
        
        if (v3Config.zones && Array.isArray(v3Config.zones)) {
            config.zones = v3Config.zones.map(zone => ({
                id: zone.id || Date.now().toString(),
                name: zone.name || 'Zona',
                type: zone.type || 0,
                positionCm: zone.positionCm || 0,
                widthCm: zone.widthCm || 30,
                heightCm: zone.heightCm || 20,
                imageSettings: {
                    imageFile: zone.imageSettings?.imageFile || '',
                    uploadedFile: null
                },
                videoSettings: {
                    videoFile: zone.videoSettings?.videoFile || '',
                    uploadedFile: null,
                    loop: zone.videoSettings?.loop !== false
                },
                textSettings: {
                    text: zone.textSettings?.text || 'Texto da zona',
                    fontSize: zone.textSettings?.fontSize || 24,
                    textColor: zone.textSettings?.textColor || [0, 0, 0, 1]
                }
            }));
        }
        
        return config;
    }

    arrayToHexColor(colorArray) {
        if (!Array.isArray(colorArray) || colorArray.length < 3) return '#000000';
        
        const r = Math.round(colorArray[0] * 255);
        const g = Math.round(colorArray[1] * 255);
        const b = Math.round(colorArray[2] * 255);
        
        return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
    }

    showLoadModal() {
        const modal = document.getElementById('load-modal');
        if (modal) {
            modal.style.display = 'block';
        }
    }

    hideLoadModal() {
        console.log('Tentando fechar modal...');
        const modal = document.getElementById('load-modal');
        console.log('Modal para fechar:', modal);
        
        if (modal) {
            modal.style.display = 'none';
            console.log('Modal fechado com sucesso');
            
            // Limpar o input de arquivo
            const fileInput = modal.querySelector('#load-json-file');
            if (fileInput) {
                fileInput.value = '';
                console.log('Input de arquivo limpo');
            }
        } else {
            console.error('Modal não encontrado para fechar!');
        }
    }

    saveConfig() {
        try {
            const configData = {
                ...this.config,
                zones: this.zones,
                lastModified: new Date().toISOString()
            };
            
            // Salvar no localStorage
            localStorage.setItem('trilho-config', JSON.stringify(configData));
            
            // Atualizar valores originais para o que foi salvo
            this.originalConfig = JSON.parse(JSON.stringify(this.config));
            this.originalZones = JSON.parse(JSON.stringify(this.zones));
            console.log('Valores originais atualizados para configuração salva');
            
            this.showToast('Configuração salva localmente com sucesso!', 'success');
            console.log('Configuração salva no localStorage');
            
            // Atualizar timestamp de última modificação
            this.updateLastModified();
            
        } catch (error) {
            console.error('Erro ao salvar configuração:', error);
            this.showToast('Erro ao salvar configuração', 'error');
        }
    }

    exportConfig() {
        try {
            const v3Config = this.convertToV3Format();
            const blob = new Blob([JSON.stringify(v3Config, null, 2)], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'trilho_v3_config.json';
            a.click();
            URL.revokeObjectURL(url);
            
            this.showToast('Configuração V3 exportada com sucesso!', 'success');
            console.log('Configuração V3 exportada');
        } catch (error) {
            console.error('Erro ao exportar configuração V3:', error);
            this.showToast('Erro ao exportar configuração V3', 'error');
        }
    }

    convertToV3Format() {
        const v3Config = {
            project: { ...this.config.project },
            trilho: { ...this.config.trilho },
            camera: { ...this.config.camera },
            osc: { ...this.config.osc },
            background: { ...this.config.background },
            zones: this.zones.map(zone => ({
                id: zone.id,
                name: zone.name,
                type: zone.type,
                positionCm: zone.positionCm,
                widthCm: zone.widthCm,
                heightCm: zone.heightCm,
                imageSettings: { ...zone.imageSettings },
                videoSettings: { ...zone.videoSettings },
                textSettings: { ...zone.textSettings }
            }))
        };
        
        return v3Config;
    }

    async saveToUnity() {
        try {
            console.log('Iniciando salvamento para Unity...');
            
            const configData = {
                ...this.config,
                zones: this.zones,
                lastModified: new Date().toISOString()
            };
            
            // Verificar se há arquivos de mídia para incluir
            const mediaFiles = this.collectMediaFiles();
            console.log('Arquivos de mídia encontrados:', mediaFiles);
            
            if (mediaFiles.length > 0) {
                // Criar pacote ZIP com JSON + arquivos
                await this.createUnityPackage(configData, mediaFiles);
                this.showToast('Pacote Unity criado com sucesso!', 'success');
            } else {
                // Apenas JSON se não houver arquivos
                this.downloadJSON(configData, 'trilho_config.json');
                this.showToast('Configuração JSON salva para Unity!', 'success');
            }
            
            // Também salvar no localStorage
            localStorage.setItem('trilho-config', JSON.stringify(configData));
            
            console.log('Configuração salva para Unity:', configData);
            
        } catch (error) {
            console.error('Erro ao salvar para Unity:', error);
            this.showToast('Erro ao salvar para Unity: ' + error.message, 'error');
        }
    }
    
    collectMediaFiles() {
        const mediaFiles = [];
        
        this.zones.forEach(zone => {
            // Verificar arquivos de imagem
            if (zone.type === 0 && zone.imageSettings?.uploadedFile) {
                mediaFiles.push({
                    name: zone.imageSettings.imageFile || 'image.jpg',
                    file: zone.imageSettings.uploadedFile,
                    type: 'image'
                });
            }
            
            // Verificar arquivos de vídeo
            if (zone.type === 1 && zone.videoSettings?.uploadedFile) {
                mediaFiles.push({
                    name: zone.videoSettings.videoFile || 'video.mp4',
                    file: zone.videoSettings.uploadedFile,
                    type: 'video'
                });
            }
        });
        
        // Verificar arquivo de background
        if (this.config.background?.uploadedFile) {
            mediaFiles.push({
                name: this.config.background.imageFile || 'background.jpg',
                file: this.config.background.uploadedFile,
                type: 'background'
            });
        }
        
        return mediaFiles;
    }
    
    async createUnityPackage(configData, mediaFiles) {
        try {
            // Usar JSZip para criar o pacote
            if (typeof JSZip === 'undefined') {
                console.warn('JSZip não disponível, criando apenas JSON');
                this.downloadJSON(configData, 'trilho_config.json');
                return;
            }
            
            const zip = new JSZip();
            
            // Adicionar JSON de configuração
            zip.file('trilho_config.json', JSON.stringify(configData, null, 2));
            
            // Adicionar arquivos de mídia
            mediaFiles.forEach(mediaFile => {
                const folder = mediaFile.type === 'background' ? 'Images' : 
                             mediaFile.type === 'video' ? 'Videos' : 'Images';
                zip.file(`${folder}/${mediaFile.name}`, mediaFile.file);
            });
            
            // Adicionar README
            const readme = this.createUnityReadme();
            zip.file('README_UNITY.txt', readme);
            
            // Gerar e baixar ZIP
            const zipBlob = await zip.generateAsync({ type: 'blob' });
            const url = URL.createObjectURL(zipBlob);
            
            const a = document.createElement('a');
            a.href = url;
            a.download = 'trilho_unity_package.zip';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            
            URL.revokeObjectURL(url);
            
            console.log('Pacote Unity criado com sucesso!');
            
        } catch (error) {
            console.error('Erro ao criar pacote Unity:', error);
            // Fallback para JSON apenas
            this.downloadJSON(configData, 'trilho_config.json');
        }
    }
    
    createUnityReadme() {
        return `TRILHO UNITY PACKAGE
=====================

Este pacote contém:
1. trilho_config.json - Configuração principal
2. Pasta Images/ - Imagens e backgrounds
3. Pasta Videos/ - Arquivos de vídeo

INSTRUÇÕES PARA UNITY:
1. Extraia o ZIP na pasta StreamingAssets do seu projeto Unity
2. Certifique-se de que as pastas Images/ e Videos/ estão em StreamingAssets
3. Use o TrilhoConfigLoader.cs para carregar trilho_config.json
4. Os arquivos de mídia serão carregados automaticamente

OBSERVAÇÕES:
- Mantenha a estrutura de pastas intacta
- Verifique se os caminhos no JSON correspondem às pastas
- Para NDI, use apenas números inteiros (sem decimais)

Versão: ${this.config.project.version || '1.0'}
Data: ${new Date().toLocaleDateString('pt-BR')}
`;
    }
    
    downloadJSON(configData, filename) {
        const jsonString = JSON.stringify(configData, null, 2);
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        
        URL.revokeObjectURL(url);
    }

    loadStoredConfig() {
        console.log('=== LOAD STORED CONFIG ===');
        console.log('Zonas atuais:', this.zones.length);
        
        // Só carregar do localStorage se não houver zonas já carregadas
        if (this.zones.length === 0) {
            const storedConfig = localStorage.getItem('trilho-config');
            console.log('Configuração no localStorage:', storedConfig ? 'encontrada' : 'não encontrada');
            
            if (storedConfig) {
                try {
                    const configData = JSON.parse(storedConfig);
                    console.log('Configuração parseada do localStorage:', configData);
                    console.log('Zonas no localStorage:', configData.zones ? configData.zones.length : 'não definido');
                    
                    this.loadConfigFromJSON(configData);
                    console.log('Configuração carregada do localStorage');
                } catch (error) {
                    console.error('Erro ao carregar configuração armazenada:', error);
                }
            } else {
                console.log('Nenhuma configuração encontrada no localStorage');
            }
        } else {
            console.log('Zonas já carregadas, pulando localStorage');
        }
        
        console.log('=== FIM LOAD STORED CONFIG ===');
    }

    populateFormFields() {
        console.log('=== POPULANDO CAMPOS ===');
        console.log('Configuração atual:', this.config);
        
        // Project fields
        const projectName = document.getElementById('project-name');
        if (projectName) projectName.value = this.config.project.name || '';
        
        const projectDescription = document.getElementById('project-description');
        if (projectDescription) projectDescription.value = this.config.project.description || '';
        
        const projectVersion = document.getElementById('project-version');
        if (projectVersion) projectVersion.value = this.config.project.version || '';

        // Trilho fields
        console.log('=== PREENCHENDO CAMPOS TRILHO ===');
        console.log('Configuração atual do trilho:', this.config.trilho);
        const trilhoFields = [
            { sliderId: 'physical-min-cm-slider', inputId: 'physical-min-cm', value: this.config.trilho.physicalMinCm },
            { sliderId: 'physical-max-cm-slider', inputId: 'physical-max-cm', value: this.config.trilho.physicalMaxCm },
            { sliderId: 'unity-min-position-slider', inputId: 'unity-min-position', value: this.config.trilho.unityMinPosition },
            { sliderId: 'unity-max-position-slider', inputId: 'unity-max-position', value: this.config.trilho.unityMaxPosition },
            { sliderId: 'screen-width-cm-slider', inputId: 'screen-width-cm', value: this.config.trilho.screenWidthCm },
            { sliderId: 'movement-sensitivity-slider', inputId: 'movement-sensitivity', value: this.config.trilho.movementSensitivity }
        ];

        trilhoFields.forEach(({ sliderId, inputId, value }) => {
            const slider = document.getElementById(sliderId);
            const input = document.getElementById(inputId);
            console.log(`Campo ${inputId}:`, { value, slider: !!slider, input: !!input });
            if (slider) {
                slider.value = value;
                console.log(`Slider ${sliderId} definido para: ${value}`);
            }
            if (input) {
                input.value = value;
                console.log(`Input ${inputId} definido para: ${value}`);
            }
        });

        // Camera fields
        console.log('=== PREENCHENDO CAMPOS CÂMERA ===');
        const cameraSmoothingSlider = document.getElementById('camera-smoothing-slider');
        const cameraSmoothingInput = document.getElementById('camera-smoothing');
        console.log('Camera smoothing:', { value: this.config.camera.cameraSmoothing, slider: !!cameraSmoothingSlider, input: !!cameraSmoothingInput });
        if (cameraSmoothingSlider) cameraSmoothingSlider.value = this.config.camera.cameraSmoothing;
        if (cameraSmoothingInput) cameraSmoothingInput.value = this.config.camera.cameraSmoothing;

        // OSC fields
        console.log('=== PREENCHENDO CAMPOS OSC ===');
        const oscAddress = document.getElementById('osc-address');
        const oscPort = document.getElementById('osc-port');
        console.log('OSC address:', { value: this.config.osc.address, input: !!oscAddress });
        console.log('OSC port:', { value: this.config.osc.port, input: !!oscPort });
        if (oscAddress) oscAddress.value = this.config.osc.address || '';
        if (oscPort) oscPort.value = this.config.osc.port;

        // Background fields
        console.log('=== PREENCHENDO CAMPOS BACKGROUND ===');
        const backgroundEnabled = document.getElementById('background-enabled');
        if (backgroundEnabled) backgroundEnabled.checked = this.config.background.enabled;
        
        const backgroundImageFile = document.getElementById('background-image-file');
        if (backgroundImageFile) backgroundImageFile.value = this.config.background.imageFile || '';
        
        console.log('=== CAMPOS POPULADOS ===');
    }

    renderZones() {
        const zonesContainer = document.getElementById('zones-container');
        if (zonesContainer) {
            zonesContainer.innerHTML = '';
            this.zones.forEach((zone, index) => this.renderZone(zone, index));
        }
        
        // O minimapa será atualizado quando a seção zones for ativada
    }

    initializeResetButtons() {
        console.log('Inicializando botões de reset...');
        const resetButtons = document.querySelectorAll('.reset-section-btn');
        console.log(`Encontrados ${resetButtons.length} botões de reset:`, resetButtons);
        
        resetButtons.forEach((button, index) => {
            const section = button.getAttribute('data-section');
            const buttonId = button.id;
            console.log(`Configurando botão ${index + 1}: ${buttonId} -> seção: ${section}`);
            
            button.addEventListener('click', () => {
                console.log(`Botão de reset clicado: ${buttonId} para seção: ${section}`);
                this.resetSection(section);
            });
        });
        
        console.log('Botões de reset inicializados com sucesso');
    }

    initializeStickyActions() {
        console.log('Inicializando ações sticky...');
        
        // Só criar sticky se estivermos em uma seção que tenha ações
        const activeSection = document.querySelector('.content-section.active');
        if (!activeSection) {
            console.log('Nenhuma seção ativa para criar sticky');
            return;
        }
        
        const sectionActions = activeSection.querySelector('.section-actions');
        if (!sectionActions) {
            console.log('Seção ativa não tem ações para sticky');
            return;
        }
        
        // Implementação alternativa usando JavaScript para simular sticky
        this.setupJavaScriptSticky();
        
        console.log('Ações sticky inicializadas com sucesso');
    }

    setupJavaScriptSticky() {
        console.log('=== CONFIGURANDO STICKY VIA JAVASCRIPT ===');
        
        // Limpar clones existentes
        this.clearStickyClones();
        
        // Só criar clones para a seção ativa
        const activeSection = document.querySelector('.content-section.active');
        if (!activeSection) {
            console.warn('❌ Nenhuma seção ativa encontrada');
            return;
        }
        
        console.log('✅ Seção ativa encontrada:', activeSection.id);
        
        const sectionActions = activeSection.querySelector('.section-actions');
        if (!sectionActions) {
            console.warn('❌ Ações da seção ativa não encontradas');
            return;
        }
        
        console.log('✅ Ações da seção encontradas:', sectionActions);
        
        // Criar clone fixo dos botões
        const stickyActions = sectionActions.cloneNode(true);
        stickyActions.className = 'section-actions section-actions-sticky';
        stickyActions.id = 'sticky-actions-clone';
        
        // Obter a posição e dimensões exatas dos botões originais
        const originalRect = sectionActions.getBoundingClientRect();
        
        console.log('📐 Dimensões originais:', {
            rect: originalRect,
            left: originalRect.left,
            width: originalRect.width,
            height: originalRect.height
        });
        
        // Estilos simples - apenas posição fixa e top
        stickyActions.style.cssText = `
            position: fixed !important;
            top: 70px !important;
            left: ${originalRect.left}px !important;
            background: var(--bg-primary) !important;
            border-bottom: 1px solid var(--border-color) !important;
            box-shadow: var(--shadow-md) !important;
            z-index: 9999 !important;
            display: none !important;
        `;
        
        // Adicionar ao body
        document.body.appendChild(stickyActions);
        
        // Armazenar referência para controle
        this.currentStickyClone = stickyActions;
        
        // Verificar se foi adicionado corretamente
        const cloneAdded = document.body.contains(stickyActions);
        console.log('✅ Sticky clone criado:', stickyActions);
        console.log('✅ Estilos aplicados:', stickyStyles);
        console.log('📍 Clone adicionado ao DOM:', cloneAdded);
        
        // Verificar se está visível
        const initialStyle = window.getComputedStyle(stickyActions);
        console.log('🔍 Estilo inicial do clone:', {
            display: initialStyle.display,
            position: initialStyle.position,
            zIndex: initialStyle.zIndex,
            top: initialStyle.top,
            left: initialStyle.left
        });
        
        // Adicionar listener de scroll
        if (!this.scrollListenerAdded) {
            window.addEventListener('scroll', this.handleScroll.bind(this));
            this.scrollListenerAdded = true;
            console.log('✅ Listener de scroll adicionado');
        }
        
        console.log('=== STICKY CONFIGURADO COM SUCESSO ===');
    }

    clearStickyClones() {
        console.log('🧹 Limpando clones sticky...');
        
        // Remover clones existentes
        const existingClones = document.querySelectorAll('.section-actions-sticky');
        console.log(`📋 Encontrados ${existingClones.length} clones para remover`);
        
        existingClones.forEach((clone, index) => {
            clone.remove();
            console.log(`🗑️ Clone ${index + 1} removido:`, clone);
        });
        
        // Limpar referências
        this.currentStickyClone = null;
        
        console.log('✅ Clones sticky limpos');
    }

    handleScroll() {
        console.log('🔄 Scroll detectado');
        
        // Só funcionar se tivermos um clone sticky ativo
        if (!this.currentStickyClone) {
            console.log('❌ Nenhum clone sticky ativo');
            return;
        }
        
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const headerHeight = 70;
        
        console.log('📊 Dados do scroll:', { scrollTop, headerHeight });
        
        // Encontrar a seção ativa
        const activeSection = document.querySelector('.content-section.active');
        if (!activeSection) {
            console.log('❌ Nenhuma seção ativa encontrada');
            return;
        }
        
        const sectionActions = activeSection.querySelector('.section-actions');
        if (!sectionActions) {
            console.log('❌ Ações da seção não encontradas');
            return;
        }
        
        // Verificar a posição do próprio .section-actions
        const actionsRect = sectionActions.getBoundingClientRect();
        const actionsTop = actionsRect.top;
        
        console.log('📍 Posição das ações:', { 
            actionsTop, 
            headerHeight, 
            shouldShow: actionsTop <= headerHeight,
            actionsRect: actionsRect
        });
        
        // Mostrar sticky quando o .section-actions original saiu da tela
        if (actionsTop <= headerHeight) {
            this.currentStickyClone.style.display = 'flex';
            this.currentStickyClone.classList.add('sticky-active');
            console.log('✅ Sticky ATIVADO - section-actions saiu da tela');
        } else {
            this.currentStickyClone.style.display = 'none';
            this.currentStickyClone.classList.remove('sticky-active');
            console.log('❌ Sticky DESATIVADO - section-actions visível');
        }
    }

    testSticky() {
        console.log('🧪 === TESTE DE STICKY ===');
        
        // Aguardar um pouco para o DOM estar pronto
        setTimeout(() => {
            console.log('🔍 Iniciando teste de sticky...');
            
            // Verificar seção ativa
            const activeSection = document.querySelector('.content-section.active');
            console.log('📋 Seção ativa:', activeSection?.id || 'Nenhuma');
            
            // Verificar ações da seção
            const sectionActions = activeSection?.querySelector('.section-actions');
            console.log('🔘 Ações da seção:', sectionActions || 'Não encontradas');
            
            // Verificar se o sticky foi criado
            const stickyClone = document.querySelector('.section-actions-sticky');
            console.log('📌 Clone sticky:', stickyClone || 'Não criado');
            
            if (stickyClone) {
                const computedStyle = window.getComputedStyle(stickyClone);
                console.log('🎨 Estilos do clone sticky:', {
                    position: computedStyle.position,
                    top: computedStyle.top,
                    left: computedStyle.left,
                    zIndex: computedStyle.zIndex,
                    display: computedStyle.display
                });
            }
            
            // Verificar se o listener de scroll está ativo
            console.log('🎧 Listener de scroll ativo:', this.scrollListenerAdded);
            
            // Testar scroll manual
            console.log('📜 Testando scroll manual...');
            window.dispatchEvent(new Event('scroll'));
            
            console.log('✅ Teste de sticky concluído');
        }, 1000);
    }

    resetSection(section) {
        console.log(`Resetando seção: ${section}`);
        
        // Verificar se temos valores originais do JSON
        if (!this.originalConfig) {
            console.warn('Nenhuma configuração original encontrada. Usando valores padrão.');
            const defaultConfig = this.getDefaultConfig();
            
            switch (section) {
                case 'general':
                    this.config.project = { ...defaultConfig.project };
                    break;
                case 'trilho':
                    this.config.trilho = { ...defaultConfig.trilho };
                    break;
                case 'camera':
                    this.config.camera = { ...defaultConfig.camera };
                    break;
                case 'osc':
                    this.config.osc = { ...defaultConfig.osc };
                    break;
                case 'background':
                    this.config.background = { ...defaultConfig.background };
                    break;
                case 'zones':
                    this.zones = [];
                    break;
            }
            
            this.showToast(`Seção ${section} resetada para valores padrão`, 'info');
        } else {
            // Usar valores originais do JSON
            console.log('Usando valores originais do JSON para reset');
            
            switch (section) {
                case 'general':
                    this.config.project = { ...this.originalConfig.project };
                    console.log('Projeto resetado para valores originais:', this.config.project);
                    break;
                case 'trilho':
                    this.config.trilho = { ...this.originalConfig.trilho };
                    console.log('Trilho resetado para valores originais:', this.config.trilho);
                    break;
                case 'camera':
                    this.config.camera = { ...this.originalConfig.camera };
                    console.log('Câmera resetada para valores originais:', this.config.camera);
                    break;
                case 'osc':
                    this.config.osc = { ...this.originalConfig.osc };
                    console.log('OSC resetado para valores originais:', this.config.osc);
                    break;
                case 'background':
                    this.config.background = { ...this.originalConfig.background };
                    console.log('Background resetado para valores originais:', this.config.background);
                    break;
                case 'zones':
                    console.log('=== RESETANDO ZONAS ===');
                    console.log('Zonas originais antes do reset:', this.originalZones);
                    console.log('Zonas atuais antes do reset:', this.zones);
                    
                    // Deep copy mais robusto das zonas originais
                    if (this.originalZones && this.originalZones.length > 0) {
                        this.zones = this.originalZones.map(zone => ({
                            ...zone,
                            id: zone.id,
                            name: zone.name,
                            type: zone.type,
                            positionCm: zone.positionCm || 0,
                            widthCm: zone.widthCm || 800,
                            heightCm: zone.heightCm || 600,
                            imageSettings: zone.imageSettings ? { ...zone.imageSettings } : {},
                            videoSettings: zone.videoSettings ? { ...zone.videoSettings } : {},
                            textSettings: zone.textSettings ? { ...zone.textSettings } : {}
                        }));
                        console.log('Zonas após reset (deep copy robusto):', this.zones);
                        
                        // Verificar se as propriedades estão corretas
                        this.zones.forEach((zone, index) => {
                            console.log(`Zona ${index + 1} após reset:`, {
                                name: zone.name,
                                widthCm: zone.widthCm,
                                heightCm: zone.heightCm,
                                positionCm: zone.positionCm
                            });
                        });
                    } else {
                        console.warn('Nenhuma zona original encontrada para reset');
                        this.zones = [];
                    }
                    break;
                default:
                    console.warn(`Seção desconhecida para reset: ${section}`);
                    return;
            }
            
            this.showToast(`Seção ${section} resetada para valores originais do JSON`, 'info');
        }
        
        console.log('Atualizando campos do formulário...');
        this.populateFormFields();
        
        // Para zonas, também re-renderizar e atualizar minimapa
        if (section === 'zones') {
            console.log('Re-renderizando zonas após reset...');
            this.renderZones();
            setTimeout(() => {
                this.updateTrilhoMinimap();
                console.log('Minimapa atualizado após reset das zonas');
            }, 100);
        }
        
        this.updateLastModified();
        console.log(`Seção ${section} resetada com sucesso`);
    }

    updateLastModified() {
        this.config.lastModified = new Date().toISOString();
        const lastModifiedElement = document.getElementById('last-modified');
        if (lastModifiedElement) {
            lastModifiedElement.textContent = new Date(this.config.lastModified).toLocaleString();
        }
    }

    showToast(message, type = 'info') {
        const toastContainer = document.getElementById('toast-container');
        if (toastContainer) {
            const toast = document.createElement('div');
            toast.className = `toast toast-${type}`;
            toast.textContent = message;
            
            toastContainer.appendChild(toast);
            
            // Show toast
            setTimeout(() => toast.classList.add('show'), 100);
            
            // Hide and remove toast
            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => toast.remove(), 300);
            }, 3000);
            
            console.log('Toast mostrado com sucesso');
        } else {
            console.error('Container de toast não encontrado');
        }
    }

    /**
     * Adiciona event listeners para configurações específicas de texto
     */
    addTextSettingsEventListeners(zoneElement, zone) {
        // Campo de texto
        const textContent = zoneElement.querySelector('.zone-text-content');
        if (textContent) {
            textContent.addEventListener('input', (e) => {
                if (!zone.textSettings) zone.textSettings = {};
                zone.textSettings.text = e.target.value;
                this.updateLastModified();
            });
        }
        
        // Tamanho da fonte
        const fontSizeSlider = zoneElement.querySelector('.zone-fontsize-slider');
        const fontSizeInput = zoneElement.querySelector('.zone-fontsize-input');
        if (fontSizeSlider && fontSizeInput) {
            fontSizeSlider.addEventListener('input', (e) => {
                if (!zone.textSettings) zone.textSettings = {};
                zone.textSettings.fontSize = parseInt(e.target.value);
                fontSizeInput.value = e.target.value;
                this.updateLastModified();
            });
            
            fontSizeInput.addEventListener('input', (e) => {
                if (!zone.textSettings) zone.textSettings = {};
                zone.textSettings.fontSize = parseInt(e.target.value);
                fontSizeSlider.value = e.target.value;
                this.updateLastModified();
            });
        }
        
        // Cor do texto
        const textColor = zoneElement.querySelector('.zone-text-color');
        if (textColor) {
            textColor.addEventListener('change', (e) => {
                if (!zone.textSettings) zone.textSettings = {};
                // Converter cor hex para array RGB
                const hex = e.target.value;
                const r = parseInt(hex.substr(1, 2), 16) / 255;
                const g = parseInt(hex.substr(3, 2), 16) / 255;
                const b = parseInt(hex.substr(5, 2), 16) / 255;
                zone.textSettings.textColor = [r, g, b, 1];
                this.updateLastModified();
            });
        }
    }

    /**
     * Mostra/oculta campos específicos baseado no tipo de conteúdo selecionado
     */
    showZoneTypeSettings(zoneElement, contentType) {
        console.log('Mostrando configurações para tipo:', contentType);
        
        // Ocultar todas as configurações específicas primeiro
        const allTypeSettings = zoneElement.querySelectorAll('.zone-type-settings > div');
        console.log('Configurações encontradas:', allTypeSettings.length);
        allTypeSettings.forEach(div => {
            div.style.display = 'none';
            console.log('Ocultando:', div.className);
        });
        
        // Mostrar configurações específicas baseado no tipo
        switch (contentType) {
            case 0: // Imagem
                const imageSettings = zoneElement.querySelector('.zone-image-settings');
                if (imageSettings) {
                    imageSettings.style.display = 'block';
                    console.log('Mostrando configurações de imagem');
                }
                break;
            case 1: // Vídeo
                const videoSettings = zoneElement.querySelector('.zone-video-settings');
                if (videoSettings) {
                    videoSettings.style.display = 'block';
                    console.log('Mostrando configurações de vídeo');
                }
                break;
            case 2: // Texto
                const textSettings = zoneElement.querySelector('.zone-text-settings');
                if (textSettings) {
                    textSettings.style.display = 'block';
                    console.log('Mostrando configurações de texto');
                }
                break;
            case 3: // Aplicação
                console.log('Tipo aplicação - sem configurações específicas');
                break;
        }
        
        // Mostrar o container de configurações específicas
        const typeSettingsContainer = zoneElement.querySelector('.zone-type-settings');
        if (typeSettingsContainer) {
            typeSettingsContainer.style.display = 'block';
            console.log('Container de configurações específicas exibido');
        } else {
            console.error('Container de configurações específicas não encontrado');
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new TrilhoConfigurator();
});

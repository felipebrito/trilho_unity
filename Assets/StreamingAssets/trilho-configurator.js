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
        this.hasUserInteracted = false; // Flag para controlar validação automática
        this.isManualValidation = false; // Flag para controlar se é validação manual
        console.log('Configuração padrão criada:', this.config);
        this.initializeEventListeners();
        
        // Load stored config after DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                // Não carregar automaticamente do localStorage
                // this.loadStoredConfig();
                this.updateLastModified();
                this.showSection('general');
                // Atualizar minimapa após DOM estar pronto
                setTimeout(() => this.updateTrilhoMinimap(), 1000);
            });
        } else {
            // Não carregar automaticamente do localStorage
            // this.loadStoredConfig();
            this.updateLastModified();
            this.showSection('general');
            // Atualizar minimapa após DOM estar pronto
            setTimeout(() => this.updateTrilhoMinimap(), 1000);
        }
        
        // Verificar uso do localStorage
        this.checkLocalStorageUsage();
        
        // Limpeza preventiva para evitar problemas de quota
        this.forceCleanupLocalStorage();
        
        // Verificar se ainda há problemas de quota após limpeza
        try {
            // Tentar salvar um item de teste para verificar se há espaço
            const testKey = 'trilho-test-' + Date.now();
            localStorage.setItem(testKey, 'test');
            localStorage.removeItem(testKey);
        } catch (error) {
            if (error.name === 'QuotaExceededError') {
                console.warn('⚠️ localStorage ainda cheio após limpeza, forçando limpeza total...');
                this.clearAllData();
            }
        }
        
        // Tornar acessível globalmente para os modais
        window.trilhoConfigurator = this;
        
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
                eventLocation: '',
                eventDate: '',
                clientName: '',
                technicalResponsible: ''
            },
            trilho: {
                physicalMinCm: 0,
                physicalMaxCm: 300,
                tvHeightFromFloor: 80,
                screenWidthCm: 0, // Calculado automaticamente
                screenHeightCm: 0, // Calculado automaticamente
                movementSensitivity: 0.5,
                cameraSmoothing: 5,
                preActivationRange: 20 // Percentagem
            },
            tv: {
                model: '42',
                orientation: 'portrait',
                screenWidthCm: 52.5,  // TV 42" vertical: 52.5cm (altura da TV)
                screenHeightCm: 93.5, // TV 42" vertical: 93.5cm (largura da TV)
                resolution: '1920x1080'
            },
            osc: {
                address: '/unity',
                port: 9000,
                valorNormalizado01: true
            },
            background: {
                enabled: true,
                imageFile: '',
                uploadedFile: null,
                widthCm: 300,
                heightCm: 200,
                offsetX: 0,
                offsetY: 0,
                displayMode: 'fit',
                opacity: 100,
                tint: '#ffffff',
                parallax: false
            },
            zones: [],
            lastModified: new Date().toISOString(),
            creationDate: new Date().toISOString()
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

        // TV calculations (com delay para garantir que o DOM esteja pronto)
        setTimeout(() => {
            this.initializeTVCalculations();
            // Recalcular após popular os campos
            setTimeout(() => {
                this.calculateTVDimensions();
            }, 50);
        }, 100);

        // Validation
        this.initializeValidation();

        // Export functionality
        this.initializeExport();

        // Zone management
        document.getElementById('add-zone-btn').addEventListener('click', () => this.addZone());

        // Wizard functionality
        this.initializeWizard();
        
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
        const generalInputs = [
            'project-name', 'project-description',
            'event-location', 'event-date', 'client-name', 'technical-responsible'
        ];
        generalInputs.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.addEventListener('input', (e) => this.updateConfigValue(e.target.id, e.target.value));
                element.addEventListener('change', (e) => this.updateConfigValue(e.target.id, e.target.value));
            }
        });

        // TV configuration inputs
        const tvInputs = ['tv-model', 'tv-orientation', 'screen-resolution'];
        tvInputs.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
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
            { sliderId: 'tv-height-from-floor-slider', inputId: 'tv-height-from-floor', configPath: 'trilho.tvHeightFromFloor' },
            { sliderId: 'movement-sensitivity-slider', inputId: 'movement-sensitivity', configPath: 'trilho.movementSensitivity' },
            { sliderId: 'camera-smoothing-slider', inputId: 'camera-smoothing', configPath: 'trilho.cameraSmoothing' },
            { sliderId: 'pre-activation-range-slider', inputId: 'pre-activation-range', configPath: 'trilho.preActivationRange' }
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

        // Camera sliders (removido - configuração feita na Unity)
        const cameraSliders = [];

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
        
        // Marcar que o usuário interagiu
        this.hasUserInteracted = true;
        
        // Mapear campos específicos
        const fieldMappings = {
            'project-name': 'project.name',
            'project-description': 'project.description',
            'event-location': 'project.eventLocation',
            'event-date': 'project.eventDate',
            'client-name': 'project.clientName',
            'technical-responsible': 'project.technicalResponsible',
            'tv-model': 'tv.model',
            'tv-orientation': 'tv.orientation',
            'screen-resolution': 'tv.resolution'
        };
        
        if (fieldMappings[id]) {
            const pathParts = fieldMappings[id].split('.');
            let current = this.config;
            
            for (let i = 0; i < pathParts.length - 1; i++) {
                if (!current[pathParts[i]]) {
                    current[pathParts[i]] = {};
                }
                current = current[pathParts[i]];
            }
            
            current[pathParts[pathParts.length - 1]] = value;
            this.updateLastModified();
            console.log(`Configuração atualizada: ${fieldMappings[id]} = ${value}`);
        } else {
            // Fallback para o método antigo
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
    }

    updateConfigValueByPath(path, value) {
        console.log(`Atualizando configuração por caminho: ${path} = ${value}`);
        
        // Marcar que o usuário interagiu
        this.hasUserInteracted = true;
        
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
            
                    // Se for a seção zones, atualizar o minimapa e carregar imagem
        if (sectionName === 'zones') {
            console.log('Seção zones ativada, atualizando minimapa e carregando imagem...');
            
            // Aguardar um pouco para garantir que o DOM está pronto
            setTimeout(() => {
                console.log('🕐 Carregando background após delay...');
                this.loadBackgroundImageForZones();
            }, 100);
            
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
        // Abrir modal para nova zona
        this.openZoneModal();
    }

    addTestZone() {
        console.log('🔧 Criando zona de teste...');
        
        // Calcular posição máxima permitida
        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const tvWidth = this.config.trilho.screenWidthCm || 52.5;
        const maxPosition = trilhoMax - tvWidth;
        const testPosition = Math.max(trilhoMin, Math.min(maxPosition, 150)); // 150cm ou posição máxima
        
        const testZone = {
            id: 'test-zone-1',
            name: 'Zona Teste',
            type: 0,
            positionCm: testPosition,
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
                text: 'Zona Teste',
                fontSize: 24,
                textColor: [0, 0, 0, 1]
            }
        };

        this.zones.push(testZone);
        console.log('✅ Zona de teste adicionada:', testZone);
        console.log('Zonas agora:', this.zones);
        console.log(`Posição da zona teste: ${testPosition}cm (máximo: ${maxPosition}cm)`);
        
        // Atualizar minimapa novamente
        this.updateTrilhoMinimap();
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
                            <input type="range" class="zone-position-slider" min="${this.config.trilho.physicalMinCm || 0}" max="${(this.config.trilho.physicalMaxCm || 300) - (this.config.trilho.screenWidthCm || 52.5)}" step="1" value="${zone.positionCm || 0}">
                            <input type="number" class="zone-position-input form-input" value="${zone.positionCm || 0}" step="0.1" min="${this.config.trilho.physicalMinCm || 0}" max="${(this.config.trilho.physicalMaxCm || 300) - (this.config.trilho.screenWidthCm || 52.5)}">
                        </div>
                    </div>
                </div>
                
                <div class="zone-type-settings">
                    <div class="zone-image-settings" style="display: ${zone.type === 0 ? 'block' : 'none'};">
                        <div class="form-group">
                            <label class="form-label">Arquivo de Imagem (1080x1920px)</label>
                            <input type="file" class="zone-image-file" accept="image/*">
                            <input type="text" class="zone-image-path form-input" placeholder="Caminho da imagem" value="${zone.imageSettings?.imageFile || ''}">
                            <small class="form-help">Imagens devem ter resolução 1080x1920px</small>
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
                
                <!-- Botões de ação -->
                <div class="zone-actions">
                    <button class="btn btn-primary zone-save-btn" data-zone-id="${zone.id}">
                        <i class="fas fa-save"></i> Salvar Zona
                    </button>
                    <button class="btn btn-secondary zone-cancel-btn" data-zone-id="${zone.id}">
                        <i class="fas fa-times"></i> Cancelar
                    </button>
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

        // Save zone button
        const saveBtn = zoneElement.querySelector('.zone-save-btn');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                this.saveZone(zone);
                this.updateTrilhoMinimap();
                this.showToast('Zona salva com sucesso!', 'success');
            });
        }

        // Cancel zone button
        const cancelBtn = zoneElement.querySelector('.zone-cancel-btn');
        if (cancelBtn) {
            cancelBtn.addEventListener('click', () => {
                this.cancelZoneEdit(zoneElement, zone);
            });
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

    cancelZoneEdit(zoneElement, zone) {
        // Reverter mudanças não salvas
        const zoneIndex = this.zones.findIndex(z => z.id === zone.id);
        if (zoneIndex !== -1) {
            // Restaurar valores originais
            const originalZone = this.zones[zoneIndex];
            
            // Restaurar nome
            const zoneTitle = zoneElement.querySelector('.zone-title');
            if (zoneTitle) {
                zoneTitle.textContent = originalZone.name;
            }
            
            // Restaurar posição
            const positionSlider = zoneElement.querySelector('.zone-position-slider');
            const positionInput = zoneElement.querySelector('.zone-position-input');
            if (positionSlider && positionInput) {
                positionSlider.value = originalZone.positionCm;
                positionInput.value = originalZone.positionCm;
            }
            
            // Restaurar tipo
            const zoneTypeInput = zoneElement.querySelector('.zone-type-input');
            if (zoneTypeInput) {
                zoneTypeInput.value = originalZone.type;
                this.showZoneTypeSettings(zoneElement, originalZone.type);
            }
            
            this.showToast('Alterações canceladas', 'info');
        }
    }

    updateTrilhoMinimap() {
        console.log('=== ATUALIZANDO MINIMAPA ===');
        console.log('Configuração atual:', this.config);
        console.log('Zonas disponíveis:', this.zones);
        console.log('Tipo de this.zones:', typeof this.zones);
        console.log('Length de this.zones:', this.zones ? this.zones.length : 'undefined');
        
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
            console.log(`Marker adicionado ao DOM:`, marker);
            console.log(`Container trilho-zones agora tem ${zonesContainer.children.length} filhos`);
        });
        
        console.log(`${this.zones.length} bullets criados no minimapa`);
        console.log('=== MINIMAPA ATUALIZADO ===');
        
        // Configurar bullet movível do trilho
        this.setupTrilhoMovableBullet();
        
        this._updatingMinimap = false;
    }

    getZoneTypeName(type) {
        const types = ['Imagem', 'Vídeo', 'Texto', 'Aplicação'];
        return types[type] || 'Desconhecido';
    }

    setupTrilhoMovableBullet() {
        const bullet = document.getElementById('trilho-tv-bullet');
        if (!bullet) return;

        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const trilhoLength = trilhoMax - trilhoMin;

        let isDragging = false;
        let startX = 0;
        let startLeft = 0;

        // Função para converter posição do bullet para centímetros
        const bulletToCm = (leftPercent) => {
            return trilhoMin + (leftPercent / 100) * trilhoLength;
        };

        // Função para converter centímetros para posição do bullet
        const cmToBullet = (cm) => {
            return ((cm - trilhoMin) / trilhoLength) * 100;
        };

        // Event listeners para arrastar
        bullet.addEventListener('mousedown', (e) => {
            isDragging = true;
            startX = e.clientX;
            startLeft = parseFloat(bullet.style.left) || 50;
            bullet.style.cursor = 'grabbing';
            e.preventDefault();
        });

        document.addEventListener('mousemove', (e) => {
            if (!isDragging) return;

            const deltaX = e.clientX - startX;
            const trackWidth = 400; // Largura aproximada do trilho
            const deltaPercent = (deltaX / trackWidth) * 100;
            let newLeft = startLeft + deltaPercent;

            // Limitar entre 0% e 100%
            newLeft = Math.max(0, Math.min(100, newLeft));
            bullet.style.left = `${newLeft}%`;

            // Atualizar posição da TV em ambas as visualizações
            const tvCm = bulletToCm(newLeft);
            this.updateTVPosition(tvCm);
        });

        document.addEventListener('mouseup', () => {
            if (isDragging) {
                isDragging = false;
                bullet.style.cursor = 'pointer';
            }
        });

        // Posicionar bullet na posição atual da TV
        const currentPosition = this.config.trilho.currentPositionCm || 150;
        const initialPosition = cmToBullet(currentPosition);
        bullet.style.left = `${initialPosition}%`;
    }



    renderZonesInBackground(containerWidth, containerHeight) {
        console.log('=== RENDERIZANDO ZONAS NO BACKGROUND ===');
        console.log('Container:', containerWidth, 'x', containerHeight);
        console.log('Zonas disponíveis:', this.zones);
        console.log('Quantidade de zonas:', this.zones ? this.zones.length : 'undefined');
        
        const zonesContainer = document.getElementById('zones-trilho-zones');
        if (!zonesContainer) {
            console.log('❌ Container zones-trilho-zones não encontrado');
            console.log('Procurando por container alternativo...');
            
            // Tentar encontrar o container de outra forma
            const backgroundContainer = document.getElementById('zones-background-container');
            if (backgroundContainer) {
                console.log('✅ Container zones-background-container encontrado');
                // Criar o container se não existir
                const newContainer = document.createElement('div');
                newContainer.id = 'zones-trilho-zones';
                newContainer.className = 'zones-trilho-zones';
                backgroundContainer.appendChild(newContainer);
                console.log('✅ Container zones-trilho-zones criado');
                return this.renderZonesInBackground(); // Recursão para tentar novamente
            }
            return;
        }
        console.log('✅ Container zones-trilho-zones encontrado');

        // Limpar zonas existentes
        zonesContainer.innerHTML = '';

        if (!this.zones || this.zones.length === 0) {
            console.log('❌ Nenhuma zona para renderizar no background');
            return;
        }
        console.log('✅ Zonas encontradas para renderizar');

        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const trilhoLength = trilhoMax - trilhoMin;

        this.zones.forEach((zone, index) => {
            console.log(`🔧 Renderizando zona ${index} no background:`, zone);
            console.log(`   - Nome: ${zone.name}`);
            console.log(`   - Posição: ${zone.positionCm}cm`);
            console.log(`   - Tipo: ${zone.type}`);
            console.log(`   - ID: ${zone.id}`);
            
            const zoneElement = document.createElement('div');
            zoneElement.className = 'zones-background-zone';
            zoneElement.dataset.zoneId = zone.id;
            
            // Calcular posição percentual no eixo X
            const positionPercent = ((zone.positionCm - trilhoMin) / trilhoLength) * 100;
            console.log(`   - Posição em cm: ${zone.positionCm}cm`);
            console.log(`   - Trilho min: ${trilhoMin}cm, max: ${trilhoMax}cm, length: ${trilhoLength}cm`);
            console.log(`   - Posição percentual: ${positionPercent}%`);
            zoneElement.style.left = `${positionPercent}%`;
            
            // Posicionar no eixo Y dentro da altura da TV (centro vertical da área da TV)
            zoneElement.style.top = '50%';
            zoneElement.style.transform = 'translateY(-50%)';
            
            // Calcular tamanho baseado na TV
            const tvWidth = this.config.trilho.screenWidthCm || 52.5;
            const tvHeight = this.config.trilho.screenHeightCm || 93.5;
            const bgWidth = this.config.background.widthCm || 300;
            const bgHeight = this.config.background.heightCm || 200;
            
            // Calcular escala baseada no background
            const scaleX = containerWidth / bgWidth;
            const scaleY = containerHeight / bgHeight;
            const scale = Math.min(scaleX, scaleY);
            
            // Tamanho da zona baseado no aspecto 9:16
            const zoneHeight = 360; // Altura fixa de 360px como solicitado
            const zoneWidth = zoneHeight * (9/16); // Largura baseada no aspecto 9:16
            
            console.log(`   - TV: ${tvWidth}x${tvHeight}cm, Scale: ${scale}`);
            console.log(`   - Zona: ${zoneWidth}x${zoneHeight}px`);
            console.log(`   - Tipo: ${zone.type}, VideoData: ${!!zone.videoData}`);
            
            // Aplicar tamanho e cor baseada no tipo
            const zoneColor = this.getZoneColor(zone.type);
            const borderColor = this.getZoneTypeColor(zone.type);
            console.log(`   - Cor: ${zoneColor}, Borda: ${borderColor}`);
            
            zoneElement.style.width = `${zoneWidth}px`;
            zoneElement.style.height = `${zoneHeight}px`;
            zoneElement.style.background = zoneColor;
            zoneElement.style.borderColor = borderColor;
            
            // Conteúdo da zona
            let zoneContent = '';
            if (zone.type === 'image' && zone.imageData) {
                zoneContent = `
                    <div class="zones-zone-image-preview">
                        <img src="${zone.imageData}" alt="${zone.name}" class="zones-zone-image">
                        <div class="zones-zone-overlay">
                            <div class="zones-zone-name">${zone.name}</div>
                            <div class="zones-zone-position">${zone.positionCm}cm</div>
                        </div>
                    </div>
                `;
            } else if (zone.type === 'video' && zone.videoData) {
                console.log(`🎥 Renderizando vídeo para zona ${zone.name}:`, zone.videoData.substring(0, 50) + '...');
                zoneContent = `
                    <div class="zones-zone-video-preview">
                        <video src="${zone.videoData}" autoplay muted loop playsinline class="zones-zone-video">
                            Seu navegador não suporta vídeo.
                        </video>
                        <div class="zones-zone-overlay">
                            <div class="zones-zone-name">${zone.name}</div>
                            <div class="zones-zone-position">${zone.positionCm}cm</div>
                        </div>
                    </div>
                `;
            } else if (zone.type === 'text' && zone.text) {
                zoneContent = `
                    <div class="zones-zone-text-preview">
                        <div class="zones-zone-text-content">${zone.text}</div>
                        <div class="zones-zone-overlay">
                            <div class="zones-zone-name">${zone.name}</div>
                            <div class="zones-zone-position">${zone.positionCm}cm</div>
                        </div>
                    </div>
                `;
            } else {
                zoneContent = `
                    <div class="zones-zone-content">
                        <div class="zones-zone-name">${zone.name}</div>
                        <div class="zones-zone-position">${zone.positionCm}cm</div>
                        <div class="zones-zone-type">${this.getZoneTypeName(zone.type)}</div>
                    </div>
                `;
            }
            
            zoneElement.innerHTML = zoneContent;
            
            // Log para verificar se o vídeo foi criado
            if (zone.type === 'video' && zone.videoData) {
                const videoElement = zoneElement.querySelector('.zones-zone-video');
                if (videoElement) {
                    console.log('✅ Elemento de vídeo criado:', videoElement);
                    console.log('✅ Src do vídeo:', videoElement.src);
                } else {
                    console.log('❌ Elemento de vídeo não encontrado');
                }
            }
            
            // Event listeners
            zoneElement.addEventListener('click', (e) => {
                console.log('🖱️ Clique na zona:', zone.name);
                e.stopPropagation();
                e.preventDefault();
                this.openZoneModal(zone);
            });
            
            zoneElement.addEventListener('mouseenter', () => {
                zoneElement.classList.add('hovered');
            });
            
            zoneElement.addEventListener('mouseleave', () => {
                zoneElement.classList.remove('hovered');
            });

            // Remover drag & drop - apenas clique para abrir modal
            // this.setupZoneDragDrop(zoneElement, zone, trilhoMin, trilhoLength);
            
            // Adicionar cursor pointer para indicar que é clicável
            zoneElement.style.cursor = 'pointer';
            
            zonesContainer.appendChild(zoneElement);
            console.log(`✅ Zona ${zone.name} renderizada no background na posição ${positionPercent}%`);
            console.log(`   - Elemento adicionado ao DOM:`, zoneElement);
        });
        
        console.log(`✅ ${this.zones.length} zonas renderizadas no background`);
        console.log(`   - Container final tem ${zonesContainer.children.length} filhos`);
        console.log(`   - HTML do container:`, zonesContainer.innerHTML);
    }

    setupZoneDragDrop(zoneElement, zone, trilhoMin, trilhoLength) {
        let isDragging = false;
        let startX = 0;
        let startLeft = 0;

        // Função para converter posição do mouse para centímetros
        const mouseToCm = (mouseX, containerWidth) => {
            const percent = (mouseX / containerWidth) * 100;
            return trilhoMin + (percent / 100) * trilhoLength;
        };

        // Função para converter centímetros para posição percentual
        const cmToPercent = (cm) => {
            return ((cm - trilhoMin) / trilhoLength) * 100;
        };

        zoneElement.addEventListener('mousedown', (e) => {
            isDragging = true;
            startX = e.clientX;
            startLeft = parseFloat(zoneElement.style.left) || 0;
            zoneElement.style.cursor = 'grabbing';
            zoneElement.style.zIndex = '100'; // Trazer para frente
            e.preventDefault();
            e.stopPropagation();
        });

        document.addEventListener('mousemove', (e) => {
            if (!isDragging) return;

            const deltaX = e.clientX - startX;
            const container = document.getElementById('zones-background-container');
            const containerWidth = container ? container.offsetWidth : 800;
            
            const deltaPercent = (deltaX / containerWidth) * 100;
            let newLeft = startLeft + deltaPercent;

            // Limitar entre 0% e 100%
            newLeft = Math.max(0, Math.min(100, newLeft));
            zoneElement.style.left = `${newLeft}%`;

            // Atualizar posição da zona
            const newPositionCm = mouseToCm(e.clientX - container.getBoundingClientRect().left, containerWidth);
            zone.positionCm = Math.max(trilhoMin, Math.min(trilhoMin + trilhoLength, newPositionCm));
            
            // Atualizar tooltip
            const positionElement = zoneElement.querySelector('.zones-zone-position');
            if (positionElement) {
                positionElement.textContent = `${zone.positionCm.toFixed(1)}cm`;
            }
        });

        document.addEventListener('mouseup', () => {
            if (isDragging) {
                isDragging = false;
                zoneElement.style.cursor = 'pointer';
                zoneElement.style.zIndex = '50'; // Voltar ao z-index normal
                
                // Salvar mudanças
                this.saveZone(zone);
                this.updateTrilhoMinimap();
                this.showToast(`Zona ${zone.name} movida para ${zone.positionCm.toFixed(1)}cm`, 'success');
            }
        });
    }


    focusZone(zoneId) {
        // Focar na zona específica
        console.log('Focando na zona:', zoneId);
        
        const zone = this.zones.find(z => z.id === zoneId);
        if (!zone) {
            console.log('Zona não encontrada:', zoneId);
            return;
        }
        
        // Encontrar o elemento da zona no formulário
        const zoneElement = document.querySelector(`[data-zone-id="${zoneId}"]`);
        if (zoneElement) {
        // Expandir a zona se estiver colapsada
            const contentElement = zoneElement.querySelector('.zone-content');
            if (contentElement && contentElement.style.display === 'none') {
            this.toggleZone(zoneElement);
        }
        
            // Scroll para a zona
            zoneElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
            
            // Destacar a zona
            zoneElement.style.border = '2px solid #3b82f6';
            zoneElement.style.boxShadow = '0 0 10px rgba(59, 130, 246, 0.5)';
            
            // Remover destaque após 3 segundos
        setTimeout(() => {
                zoneElement.style.border = '';
                zoneElement.style.boxShadow = '';
            }, 3000);
        
            this.showToast(`Focando na zona: ${zone.name}`, 'info');
        }
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
        const backgroundUpload = document.getElementById('background-image-upload');

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
            console.log('✅ Campo de upload encontrado:', backgroundUpload);
            backgroundUpload.addEventListener('change', (e) => {
                console.log('📁 Arquivo selecionado:', e.target.files[0]);
                const file = e.target.files[0];
                if (file) {
                    console.log('📁 Processando arquivo:', file.name, file.type, file.size);
                    this.config.background.uploadedFile = file;
                    this.config.background.imageFile = file.name;
                    document.getElementById('background-file-name').textContent = file.name;
                    
                    // Calcular proporções da imagem automaticamente
                    this.calculateImageDimensions(file);
                    this.updateLastModified();
                } else {
                    console.log('❌ Nenhum arquivo selecionado');
                }
            });
        } else {
            console.error('❌ Campo de upload não encontrado!');
        }

        // Event listeners para dimensões do background
        const backgroundWidth = document.getElementById('background-width-cm');
        const backgroundHeight = document.getElementById('background-height-cm');
        
        // Debounce para atualizações em tempo real
        let updateTimeout;
        const debouncedUpdate = () => {
            clearTimeout(updateTimeout);
            updateTimeout = setTimeout(() => {
                this.updateSimulationVisual();
                this.checkBackgroundDimensionsReady();
            }, 100); // 100ms de delay
        };
        
        if (backgroundWidth) {
            backgroundWidth.addEventListener('input', (e) => {
                // Atualizar configuração imediatamente
                this.config.background.widthCm = parseFloat(e.target.value) || 300;
                debouncedUpdate();
            });
        }
        
        if (backgroundHeight) {
            backgroundHeight.addEventListener('input', (e) => {
                // Atualizar configuração imediatamente
                this.config.background.heightCm = parseFloat(e.target.value) || 200;
                debouncedUpdate();
            });
        }
        
        // Event listeners para simulação
        const simulationPlay = document.getElementById('simulation-play');
        const simulationReset = document.getElementById('simulation-reset');
        
        if (simulationPlay) {
            simulationPlay.addEventListener('click', () => {
                this.simulateMovement();
            });
        }
        
        if (simulationReset) {
            simulationReset.addEventListener('click', () => {
                this.resetSimulation();
            });
        }
    }

    calculateImageDimensions(file) {
        console.log('Calculando dimensões da imagem:', file.name);
        
        const img = new Image();
        const reader = new FileReader();
        
        reader.onload = (e) => {
            img.onload = () => {
                console.log(`Dimensões da imagem: ${img.width}x${img.height} pixels`);
                
                // Calcular proporção da imagem
                const imageRatio = img.width / img.height;
                console.log(`Proporção da imagem: ${imageRatio.toFixed(2)}`);
                
                // Obter dimensões configuradas do trilho
                const trilhoWidth = this.config.trilho.physicalMaxCm || 300;
                const trilhoHeight = 200; // Altura padrão do background
                
                // Calcular dimensões proporcionais
                let calculatedWidth, calculatedHeight;
                
                if (imageRatio > (trilhoWidth / trilhoHeight)) {
                    // Imagem é mais larga - ajustar pela largura
                    calculatedWidth = trilhoWidth;
                    calculatedHeight = trilhoWidth / imageRatio;
                } else {
                    // Imagem é mais alta - ajustar pela altura
                    calculatedHeight = trilhoHeight;
                    calculatedWidth = trilhoHeight * imageRatio;
                }
                
                console.log(`Dimensões calculadas: ${calculatedWidth.toFixed(1)}cm x ${calculatedHeight.toFixed(1)}cm`);
                
                // Atualizar campos
                const widthInput = document.getElementById('background-width-cm');
                const heightInput = document.getElementById('background-height-cm');
                
                if (widthInput) {
                    const roundedWidth = parseFloat(calculatedWidth.toFixed(1));
                    widthInput.value = roundedWidth;
                    this.config.background.widthCm = roundedWidth;
                }
                
                if (heightInput) {
                    const roundedHeight = parseFloat(calculatedHeight.toFixed(1));
                    heightInput.value = roundedHeight;
                    this.config.background.heightCm = roundedHeight;
                }
                
                // Manter referência do arquivo na configuração
                this.config.background.uploadedFile = file;
                this.config.background.hasUploadedFile = true;
                
                // Comprimir e salvar imagem no localStorage para persistência
                this.compressAndSaveImage(e.target.result, file.name);
                
                // Atualizar preview
                this.updateBackgroundPreview();
                
                // Verificar proporcionalidade com o trilho
                this.checkTrilhoProportionality(calculatedWidth, calculatedHeight);
                
                // Atualizar simulação visual com delay para garantir que a imagem seja carregada
                setTimeout(() => {
                    this.updateSimulationVisual();
                    console.log('Simulação visual atualizada após upload da imagem');
                }, 200);
                
                this.showToast(`Imagem carregada: ${calculatedWidth.toFixed(1)}cm x ${calculatedHeight.toFixed(1)}cm`, 'success');
            };
            
            img.src = e.target.result;
        };
        
        reader.readAsDataURL(file);
    }

    checkTrilhoProportionality(width, height) {
        const trilhoWidth = this.config.trilho.physicalMaxCm || 300;
        const trilhoHeight = 200; // Altura padrão do background
        
        const widthRatio = width / trilhoWidth;
        const heightRatio = height / trilhoHeight;
        
        console.log(`Proporcionalidade: Largura ${(widthRatio * 100).toFixed(1)}%, Altura ${(heightRatio * 100).toFixed(1)}%`);
        
        if (Math.abs(widthRatio - heightRatio) > 0.1) {
            this.showToast('⚠️ A imagem não está proporcional ao trilho configurado', 'warning');
        } else {
            this.showToast('✅ Imagem proporcional ao trilho', 'success');
        }
    }

    updateBackgroundPreview() {
        const preview = document.getElementById('background-preview');
        if (preview && this.config.background.uploadedFile) {
            const reader = new FileReader();
            reader.onload = (e) => {
                preview.src = e.target.result;
                preview.style.display = 'block';
            };
            reader.readAsDataURL(this.config.background.uploadedFile);
        }
        
        // Atualizar simulação visual
        this.updateSimulationVisual();
    }

    updateSimulationVisual() {
        console.log('🔄 Atualizando simulação visual...');
        
        // Obter dimensões do background
        const bgWidth = this.config.background.widthCm || 300;
        const bgHeight = this.config.background.heightCm || 200;
        
        console.log(`Background: ${bgWidth}cm x ${bgHeight}cm`);
        
        // Calcular escala para a simulação (máximo 600px de largura)
        const maxWidth = 600;
        const scale = maxWidth / bgWidth;
        const bgSimWidth = bgWidth * scale;
        const bgSimHeight = bgHeight * scale;
        
        console.log(`Escala: ${scale.toFixed(3)}, Simulação: ${bgSimWidth.toFixed(1)}x${bgSimHeight.toFixed(1)}px`);
        
        // Atualizar informações da simulação
        this.updateSimulationInfo(bgWidth, bgHeight, scale);
        
        // Configurar container do background
        const bgContainer = document.getElementById('simulation-background');
        if (!bgContainer) {
            console.error('❌ Container simulation-background não encontrado');
            return;
        }
        
        // Configurar dimensões e estilo do container
        bgContainer.style.width = `${bgSimWidth}px`;
        bgContainer.style.height = `${bgSimHeight}px`;
        bgContainer.style.border = '2px solid #e5e7eb';
        bgContainer.style.position = 'relative';
        bgContainer.style.margin = '0 auto';
        bgContainer.style.backgroundColor = '#f9fafb';
        bgContainer.style.backgroundSize = 'cover';
        bgContainer.style.backgroundPosition = 'center';
        bgContainer.style.backgroundRepeat = 'no-repeat';
        bgContainer.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1)';
        bgContainer.style.minHeight = '200px';
        bgContainer.style.overflow = 'visible'; // Permitir que elementos saiam
        
        // Limpar conteúdo anterior
        bgContainer.innerHTML = '';
        
        // Se há imagem carregada, exibir como background
        if (this.config.background.uploadedFile && this.config.background.uploadedFile instanceof File) {
            console.log('Carregando imagem de background:', this.config.background.uploadedFile.name);
            const reader = new FileReader();
            reader.onload = (e) => {
                bgContainer.style.backgroundImage = `url(${e.target.result})`;
                bgContainer.style.backgroundColor = 'transparent';
                console.log('Imagem de background carregada com sucesso');
                
                // Criar overlay da TV após carregar a imagem
                this.createTVOverlay(bgContainer, scale);
                this.createFloorDistanceLine(bgContainer, scale);
                this.createTrackSimulation(bgContainer, scale);
            };
            reader.readAsDataURL(this.config.background.uploadedFile);
        } else if (this.config.background.imageFile && this.config.background.hasUploadedFile) {
            // Tentar carregar imagem do localStorage
            console.log('Tentando carregar imagem do localStorage:', this.config.background.imageFile);
            this.loadImageFromStorage(this.config.background.imageFile, bgContainer, scale);
        } else {
            // Adicionar placeholder quando não há imagem
            const placeholder = document.createElement('div');
            placeholder.style.cssText = `
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                text-align: center;
                color: #6b7280;
                font-size: 14px;
                pointer-events: none;
                z-index: 1;
            `;
            placeholder.innerHTML = `
                <div style="font-size: 48px; margin-bottom: 8px;">🖼️</div>
                <div>Background: ${bgWidth.toFixed(1)}cm x ${bgHeight.toFixed(1)}cm</div>
                <div style="font-size: 12px; margin-top: 4px;">Configure as dimensões e faça upload da imagem</div>
            `;
            bgContainer.appendChild(placeholder);
            console.log('Usando placeholder visual para background');
            
            // Criar overlay da TV mesmo sem imagem
            this.createTVOverlay(bgContainer, scale);
            this.createFloorDistanceLine(bgContainer, scale);
            this.createTrackSimulation(bgContainer, scale);
        }
        
        console.log(`Simulação atualizada: Background ${bgWidth}x${bgHeight}cm`);
    }
    
    updateSimulationInfo(bgWidth, bgHeight, scale) {
        // Atualizar altura da TV do piso
        const tvHeight = this.config.trilho.tvHeightFromFloor || 80;
        document.getElementById('simulation-tv-height').textContent = `${tvHeight}cm`;
        
        // Atualizar dimensões do background
        document.getElementById('simulation-bg-size').textContent = `${bgWidth.toFixed(1)}cm x ${bgHeight.toFixed(1)}cm`;
        
        // Atualizar dimensões da TV
        const tvWidthCm = 52.5;
        const tvHeightCm = 93.5;
        document.getElementById('simulation-tv-size').textContent = `${tvWidthCm}cm x ${tvHeightCm}cm`;
    }
    
    compressAndSaveImage(imageDataURL, fileName) {
        try {
            // Criar canvas para compressão
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const img = new Image();
            
            img.onload = () => {
                // Calcular dimensões otimizadas (máximo 1200px de largura)
                const maxWidth = 1200;
                const maxHeight = 800;
                let { width, height } = img;
                
                if (width > maxWidth) {
                    height = (height * maxWidth) / width;
                    width = maxWidth;
                }
                if (height > maxHeight) {
                    width = (width * maxHeight) / height;
                    height = maxHeight;
                }
                
                // Configurar canvas
                canvas.width = width;
                canvas.height = height;
                
                // Desenhar imagem comprimida
                ctx.drawImage(img, 0, 0, width, height);
                
                // Converter para JPEG com qualidade 0.8
                const compressedDataURL = canvas.toDataURL('image/jpeg', 0.8);
                
                // Tentar salvar no localStorage
                try {
                    localStorage.setItem(`trilho_image_${fileName}`, compressedDataURL);
                    console.log('✅ Imagem comprimida e salva no localStorage:', fileName);
                    console.log(`📊 Tamanho original: ${imageDataURL.length} chars, Comprimido: ${compressedDataURL.length} chars`);
                } catch (error) {
                    console.warn('⚠️ Erro ao salvar no localStorage:', error.message);
                    // Limpar localStorage antigo se necessário
                    this.clearOldImages();
                    try {
                        localStorage.setItem(`trilho_image_${fileName}`, compressedDataURL);
                        console.log('✅ Imagem salva após limpeza do localStorage');
                    } catch (retryError) {
                        console.error('❌ Falha ao salvar imagem mesmo após limpeza:', retryError.message);
                    }
                }
            };
            
            img.src = imageDataURL;
        } catch (error) {
            console.error('❌ Erro ao comprimir imagem:', error);
        }
    }
    
    clearOldImages() {
        // Limpar imagens antigas do localStorage
        const keysToRemove = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && key.startsWith('trilho_image_')) {
                keysToRemove.push(key);
            }
        }
        
        keysToRemove.forEach(key => {
            localStorage.removeItem(key);
            console.log('🗑️ Removido do localStorage:', key);
        });
    }
    
    loadImageFromStorage(imageFileName, bgContainer, scale) {
        // Tentar carregar imagem do localStorage
        const storedImageData = localStorage.getItem(`trilho_image_${imageFileName}`);
        if (storedImageData) {
            console.log('✅ Imagem encontrada no localStorage:', imageFileName);
            bgContainer.style.backgroundImage = `url(${storedImageData})`;
            bgContainer.style.backgroundSize = 'cover';
            bgContainer.style.backgroundPosition = 'center';
            bgContainer.style.backgroundColor = 'transparent';
            
            // Criar overlay da TV após carregar a imagem
            this.createTVOverlay(bgContainer, scale);
            this.createFloorDistanceLine(bgContainer, scale);
            this.createTrackSimulation(bgContainer, scale);
        } else {
            // Se não encontrar no localStorage, mostrar placeholder
            console.log('❌ Imagem não encontrada no localStorage, mostrando placeholder');
            const placeholder = document.createElement('div');
            placeholder.style.cssText = `
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                text-align: center;
                color: #6b7280;
                font-size: 14px;
                pointer-events: none;
                z-index: 1;
            `;
            placeholder.innerHTML = `
                <div style="font-size: 48px; margin-bottom: 8px;">📁</div>
                <div>Arquivo: ${imageFileName}</div>
                <div style="font-size: 12px; margin-top: 4px;">Faça upload novamente para visualizar</div>
            `;
            bgContainer.appendChild(placeholder);
            
            // Criar overlay da TV mesmo sem imagem
            this.createTVOverlay(bgContainer, scale);
            this.createFloorDistanceLine(bgContainer, scale);
            this.createTrackSimulation(bgContainer, scale);
        }
    }

    loadBackgroundImageForZones() {
        console.log('🔍 === DEBUG: loadBackgroundImageForZones ===');
        console.log('Configuração atual:', this.config);
        console.log('Background config:', this.config.background);
        
        // Procurar imagem de background de diferentes formas
        let imageFile = null;
        let storedImageData = null;
        
        // 1. Tentar imageFile primeiro
        if (this.config.background?.imageFile) {
            imageFile = this.config.background.imageFile;
            storedImageData = localStorage.getItem(`trilho_image_${imageFile}`);
            console.log('🔍 Tentativa 1 - imageFile:', imageFile, 'Encontrada:', !!storedImageData);
        }
        
        // 2. Se não encontrou, procurar por trilho-background-image
        if (!storedImageData) {
            storedImageData = localStorage.getItem('trilho-background-image');
            if (storedImageData) {
                imageFile = 'background-image';
                console.log('🔍 Tentativa 2 - trilho-background-image encontrada');
            }
        }
        
        // 3. Se ainda não encontrou, procurar qualquer chave que contenha dados de imagem
        if (!storedImageData) {
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && (key.includes('background') || key.includes('image'))) {
                    const data = localStorage.getItem(key);
                    if (data && data.startsWith('data:image/')) {
                        storedImageData = data;
                        imageFile = key;
                        console.log('🔍 Tentativa 3 - Imagem encontrada na chave:', key);
                        break;
                    }
                }
            }
        }
        
        console.log('✅ Resultado da busca:', imageFile ? 'Imagem encontrada' : 'Nenhuma imagem encontrada');

        // Valores do trilho configurados - MOVIDO PARA CIMA
        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const trilhoLength = trilhoMax - trilhoMin;

        // Criar container de visualização do background na seção zones
        const zonesSection = document.getElementById('zones-section');
        if (!zonesSection) {
            console.log('❌ Seção zones não encontrada');
            return;
        }

        // Verificar se já existe um container de visualização
        let backgroundVisualization = document.getElementById('zones-background-visualization');
        if (!backgroundVisualization) {
            // Criar container de visualização
            backgroundVisualization = document.createElement('div');
            backgroundVisualization.id = 'zones-background-visualization';
            backgroundVisualization.className = 'zones-background-visualization';
            
            // Inserir após o minimapa
            const minimapContainer = document.querySelector('.trilho-minimap-container');
            if (minimapContainer) {
                minimapContainer.insertAdjacentElement('afterend', backgroundVisualization);
            } else {
                zonesSection.insertBefore(backgroundVisualization, zonesSection.firstChild);
            }
        }

        // Configurar o container de visualização
        backgroundVisualization.innerHTML = `
            <h3>Visualização do Background</h3>
            <div class="zones-background-container" id="zones-background-container">
                <div class="zones-background-image" id="zones-background-image"></div>
                <div class="zones-background-overlay" id="zones-background-overlay">
                    <!-- Zonas sobrepostas sobre a imagem -->
                    <div class="zones-trilho-zones" id="zones-trilho-zones">
                        <!-- Zonas serão posicionadas aqui dinamicamente -->
                    </div>
                    <!-- Bullet verde movível para posição da TV (sobre a imagem) -->
                    <div class="zones-tv-bullet" id="zones-tv-bullet" style="left: 50%; top: 50%; transform: translateY(-50%);">
                        <div class="zones-tv-bullet-inner"></div>
                    </div>
                </div>
            </div>
            <!-- TV Simulation Overlay (mesmo estilo da aba BACKGROUND) -->
            <div class="simulation-tv-overlay magnifying-active" id="zones-simulation-tv-overlay">
                <div class="simulation-tv-content">
                    <span>TV</span>
                    <div class="magnifying-glass-effect"></div>
                </div>
            </div>
        `;

        // Adicionar modal de configuração de zona (se não existir)
        this.addZoneModalHTML();

        // Aplicar a imagem de background se encontrada
        const backgroundImage = document.getElementById('zones-background-image');
        if (backgroundImage && storedImageData) {
            console.log('✅ Aplicando imagem de background...');
            backgroundImage.style.backgroundImage = `url(${storedImageData})`;
            backgroundImage.style.backgroundSize = 'cover';
            backgroundImage.style.backgroundPosition = 'center';
            backgroundImage.style.backgroundRepeat = 'no-repeat';
            console.log('✅ Imagem aplicada com sucesso');
        } else {
            console.log('⚠️ Usando placeholder - nenhuma imagem encontrada');
            backgroundImage.style.backgroundColor = '#f0f0f0';
            backgroundImage.style.border = '2px dashed #ccc';
            backgroundImage.style.display = 'flex';
            backgroundImage.style.alignItems = 'center';
            backgroundImage.style.justifyContent = 'center';
            backgroundImage.innerHTML = '<div style="text-align: center; color: #666;"><p>📷 Configure uma imagem na aba "Background"</p></div>';
        }

        // Configurar dimensões do container
        const bgWidth = this.config.background.widthCm || 300;
        const bgHeight = this.config.background.heightCm || 200;
        const tvWidth = this.config.trilho.screenWidthCm || 52.5;
        const tvHeight = this.config.trilho.screenHeightCm || 93.5;
        const tvHeightFromFloor = this.config.trilho.tvHeightFromFloor || 80;

        // Calcular escala para visualização - ajustar para não estourar o layout
        const maxWidth = Math.min(1000, window.innerWidth - 100); // Máximo 1000px ou largura da tela - 100px
        const maxHeight = 600; // Altura máxima controlada
        const scaleX = maxWidth / bgWidth;
        const scaleY = maxHeight / bgHeight;
        const scale = Math.min(scaleX, scaleY);

        const containerWidth = bgWidth * scale;
        const containerHeight = bgHeight * scale;

        // Aplicar dimensões
        const container = document.getElementById('zones-background-container');
        if (container) {
            container.style.width = `${containerWidth}px`;
            container.style.height = `${containerHeight}px`;
            container.style.maxWidth = '100%';
            container.style.overflow = 'hidden';
        }

        // Posicionar TV
        const tvOverlay = document.getElementById('zones-tv-overlay');
        if (tvOverlay) {
            const tvSimWidth = tvWidth * scale;
            const tvSimHeight = tvHeight * scale;
            const tvTopFromFloor = tvHeightFromFloor + tvHeight;
            const tvTopPosition = ((bgHeight - tvTopFromFloor) / bgHeight) * containerHeight;

            tvOverlay.style.cssText = `
                position: absolute;
                width: ${tvSimWidth}px;
                height: ${tvSimHeight}px;
                top: ${tvTopPosition}px;
                left: 50%;
                transform: translateX(-50%);
                border: 3px solid #2563eb;
                background-color: rgba(37, 99, 235, 0.1);
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 12px;
                color: #2563eb;
                font-weight: bold;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                border-radius: 8px;
                z-index: 10;
            `;
        }

        // Configurar bullet verde movível
        this.setupMovableBullet(containerWidth, trilhoMin, trilhoMax);
        
        // Configurar TV simulation overlay (como na aba Background)
        console.log('🔧 Configurando TV simulation...');
        // Aguardar um pouco para garantir que o HTML foi criado
        setTimeout(() => {
            this.setupZonesTVSimulation(containerWidth, containerHeight, trilhoMin, trilhoMax);
        }, 100);

        // As zonas já foram carregadas pelo loadWizardState() no construtor
        // Se não há zonas, criar uma de teste
        if (!this.zones || this.zones.length === 0) {
            console.log('🔧 Nenhuma zona encontrada, criando zona de teste...');
            this.zones = [{
                id: 'test-zone-1',
                name: 'Zona Teste',
                type: 'image',
                positionCm: 150,
                imageData: null
            }];
            this.saveZones();
        } else {
            console.log('🔍 Zonas encontradas:', this.zones);
            this.zones.forEach((zone, index) => {
                console.log(`Zona ${index}:`, {
                    id: zone.id,
                    name: zone.name,
                    type: zone.type,
                    positionCm: zone.positionCm,
                    hasImageData: !!zone.imageData,
                    hasVideoData: !!zone.videoData,
                    hasText: !!zone.text
                });
            });
        }
        
        // Renderizar zonas existentes no background
        console.log('🔍 Chamando renderZonesInBackground...');
        console.log('Zonas antes de renderizar:', this.zones);
        this.renderZonesInBackground(containerWidth, containerHeight);

        console.log('✅ Visualização do background criada na seção zones');
    }

    loadZonesFromStorage() {
        try {
            const savedZones = localStorage.getItem('trilho-zones');
            if (savedZones) {
                this.zones = JSON.parse(savedZones);
                console.log('✅ Zonas carregadas do localStorage:', this.zones);
                
                // Debug detalhado para vídeos
                this.zones.forEach((zone, index) => {
                    console.log(`🔍 Zona ${index}:`, {
                        id: zone.id,
                        name: zone.name,
                        type: zone.type,
                        positionCm: zone.positionCm,
                        hasImageData: !!zone.imageData,
                        hasVideoData: !!zone.videoData,
                        hasText: !!zone.text,
                        videoDataLength: zone.videoData ? zone.videoData.length : 0,
                        videoDataStart: zone.videoData ? zone.videoData.substring(0, 50) + '...' : 'N/A'
                    });
                });
            } else {
                console.log('❌ Nenhuma zona encontrada no localStorage');
                this.zones = [];
            }
        } catch (error) {
            console.error('❌ Erro ao carregar zonas do localStorage:', error);
            this.zones = [];
        }
    }

    addZoneModalHTML() {
        // Verificar se o modal já existe
        if (document.getElementById('zone-config-modal')) {
            console.log('✅ Modal já existe, não criando novamente');
            return;
        }
        
        console.log('🔧 Criando modal de configuração de zona...');

        const modalHTML = `
            <!-- Modal de Configuração de Zona -->
            <div id="zone-config-modal" class="zone-modal" style="display: none;">
                <div class="zone-modal-content">
                    <div class="zone-modal-header">
                        <h3 id="zone-modal-title">Configurar Zona</h3>
                        <button id="zone-modal-close" class="zone-modal-close">&times;</button>
                    </div>
                    <div class="zone-modal-body">
                        <form id="zone-config-form">
                            <div class="form-group">
                                <label for="zone-name">Nome da Zona:</label>
                                <input type="text" id="zone-name" name="name" required>
                            </div>
                            
                            <div class="form-group">
                                <label for="zone-type">Tipo:</label>
                                <select id="zone-type" name="type" required>
                                    <option value="image">Imagem</option>
                                    <option value="video">Vídeo</option>
                                    <option value="text">Texto</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label for="zone-position">Posição (cm):</label>
                                <input type="number" id="zone-position" name="positionCm" min="0" max="300" step="0.1" required>
                            </div>
                            
                            <div class="form-group" id="zone-image-group" style="display: none;">
                                <label for="zone-image">Imagem:</label>
                                <input type="file" id="zone-image" name="image" accept="image/*">
                                <div id="zone-image-preview" class="zone-image-preview"></div>
                            </div>
                            
                            <div class="form-group" id="zone-video-group" style="display: none;">
                                <label for="zone-video">Vídeo:</label>
                                <input type="file" id="zone-video" name="video" accept="video/*">
                                <div id="zone-video-preview" class="zone-video-preview"></div>
                            </div>
                            
                            <div class="form-group" id="zone-text-group" style="display: none;">
                                <label for="zone-text">Texto:</label>
                                <textarea id="zone-text" name="text" rows="3"></textarea>
                            </div>
                        </form>
                    </div>
                    <div class="zone-modal-footer">
                        <button id="zone-modal-cancel" class="btn btn-secondary">Cancelar</button>
                        <button id="zone-modal-save" class="btn btn-primary">Salvar</button>
                    </div>
                </div>
            </div>
        `;

        // Adicionar modal ao body
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        
        // Configurar event listeners do modal
        this.setupZoneModalEvents();
    }

    setupZoneModalEvents() {
        const modal = document.getElementById('zone-config-modal');
        const closeBtn = document.getElementById('zone-modal-close');
        const cancelBtn = document.getElementById('zone-modal-cancel');
        const saveBtn = document.getElementById('zone-modal-save');
        const form = document.getElementById('zone-config-form');
        const typeSelect = document.getElementById('zone-type');

        // Fechar modal
        closeBtn.addEventListener('click', () => this.closeZoneModal());
        cancelBtn.addEventListener('click', () => this.closeZoneModal());
        
        // Fechar ao clicar fora do modal
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closeZoneModal();
            }
        });

        // Salvar zona
        saveBtn.addEventListener('click', () => this.saveZoneFromModal());

        // Mostrar/ocultar campos baseados no tipo
        typeSelect.addEventListener('change', (e) => {
            this.toggleZoneTypeFields(e.target.value);
        });
    }

    openZoneModal(zone = null) {
        console.log('🔧 Abrindo modal de zona:', zone);
        
        const modal = document.getElementById('zone-config-modal');
        const title = document.getElementById('zone-modal-title');
        const form = document.getElementById('zone-config-form');
        
        if (!modal) {
            console.error('❌ Modal não encontrado!');
            return;
        }
        
        console.log('✅ Modal encontrado, configurando...');
        
        // Armazenar zona atual para edição
        this.currentEditingZone = zone;
        
        if (zone) {
            // Editar zona existente
            title.textContent = 'Editar Zona';
            this.populateZoneForm(zone);
        } else {
            // Nova zona
            title.textContent = 'Nova Zona';
            form.reset();
            this.toggleZoneTypeFields('image');
        }
        
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }

    closeZoneModal() {
        const modal = document.getElementById('zone-config-modal');
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
        
        // Limpar zona atual sendo editada
        this.currentEditingZone = null;
    }

    populateZoneForm(zone) {
        document.getElementById('zone-name').value = zone.name || '';
        document.getElementById('zone-type').value = zone.type || 'image';
        document.getElementById('zone-position').value = zone.positionCm || 0;
        
        // Mostrar campos baseados no tipo
        this.toggleZoneTypeFields(zone.type);
        
        // Preencher campos específicos do tipo
        if (zone.type === 'image' && zone.imageData) {
            const preview = document.getElementById('zone-image-preview');
            preview.innerHTML = `<img src="${zone.imageData}" style="max-width: 200px; max-height: 150px;">`;
        } else if (zone.type === 'video' && zone.videoData) {
            const preview = document.getElementById('zone-video-preview');
            preview.innerHTML = `<video src="${zone.videoData}" style="max-width: 200px; max-height: 150px;" controls></video>`;
        } else if (zone.type === 'text' && zone.text) {
            document.getElementById('zone-text').value = zone.text;
        }
    }

    toggleZoneTypeFields(type) {
        // Ocultar todos os grupos
        document.getElementById('zone-image-group').style.display = 'none';
        document.getElementById('zone-video-group').style.display = 'none';
        document.getElementById('zone-text-group').style.display = 'none';
        
        // Mostrar grupo correspondente
        if (type === 'image') {
            document.getElementById('zone-image-group').style.display = 'block';
        } else if (type === 'video') {
            document.getElementById('zone-video-group').style.display = 'block';
        } else if (type === 'text') {
            document.getElementById('zone-text-group').style.display = 'block';
        }
    }

    async saveZoneFromModal() {
        const form = document.getElementById('zone-config-form');
        const formData = new FormData(form);
        
        // Verificar se é edição (zona atual armazenada)
        const currentZone = this.currentEditingZone;
        
        const zoneData = {
            name: formData.get('name'),
            type: formData.get('type'),
            positionCm: parseFloat(formData.get('positionCm'))
        };
        
        // Se é edição, manter o ID
        if (currentZone && currentZone.id) {
            zoneData.id = currentZone.id;
        }
        
        // Processar arquivo baseado no tipo
        if (zoneData.type === 'image') {
            const imageFile = formData.get('image');
            if (imageFile && imageFile.size > 0) {
                zoneData.imageData = await this.fileToBase64(imageFile);
            } else if (currentZone && currentZone.imageData) {
                // Manter imagem existente se não foi alterada
                zoneData.imageData = currentZone.imageData;
            }
        } else if (zoneData.type === 'video') {
            const videoFile = formData.get('video');
            if (videoFile && videoFile.size > 0) {
                // Mostrar aviso sobre tamanho do arquivo, mas permitir salvamento
                const fileSizeMB = (videoFile.size / 1024 / 1024).toFixed(1);
                if (videoFile.size > 5 * 1024 * 1024) { // 5MB
                    this.showToast(`Arquivo de vídeo grande (${fileSizeMB}MB). Processando...`, 'info');
                    // Mostrar indicador de progresso
                    const progressToast = this.showToast(`Processando vídeo de ${fileSizeMB}MB...`, 'info', 0);
                }
                zoneData.videoData = await this.fileToBase64(videoFile);
                if (videoFile.size > 5 * 1024 * 1024) {
                    this.showToast(`Vídeo de ${fileSizeMB}MB processado com sucesso!`, 'success');
                }
            } else if (currentZone && currentZone.videoData) {
                // Manter vídeo existente se não foi alterado
                zoneData.videoData = currentZone.videoData;
            }
        } else if (zoneData.type === 'text') {
            zoneData.text = formData.get('text');
        }
        
        // Salvar zona
        this.saveZone(zoneData);
        this.closeZoneModal();
    }

    fileToBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }

    saveZone(zoneData) {
        console.log('🔧 Salvando zona:', zoneData);
        console.log('🔧 VideoData presente:', !!zoneData.videoData);
        console.log('🔧 VideoData length:', zoneData.videoData ? zoneData.videoData.length : 0);
        
        // Se é uma zona existente (tem ID), atualizar
        if (zoneData.id) {
            const index = this.zones.findIndex(z => z.id === zoneData.id);
            if (index !== -1) {
                this.zones[index] = { ...this.zones[index], ...zoneData };
                console.log('✅ Zona atualizada:', this.zones[index]);
                console.log('✅ VideoData após atualização:', !!this.zones[index].videoData);
            } else {
                console.log('❌ Zona não encontrada para atualizar:', zoneData.id);
            }
        } else {
            // Nova zona
            const newZone = {
                id: Date.now().toString(),
                ...zoneData
            };
            this.zones.push(newZone);
            console.log('✅ Nova zona criada:', newZone);
            console.log('✅ VideoData na nova zona:', !!newZone.videoData);
        }
        
        // Salvar no localStorage
        this.saveZones();
        
        // Atualizar visualizações
        this.renderZones();
        this.updateTrilhoMinimap();
        
        // Re-renderizar zonas no background com dimensões corretas
        const container = document.getElementById('zones-background-container');
        if (container) {
            const containerWidth = container.offsetWidth;
            const containerHeight = container.offsetHeight;
            this.renderZonesInBackground(containerWidth, containerHeight);
        }
        
        console.log('✅ Zonas salvas e visualizações atualizadas');
    }

    saveZones() {
        try {
            // Salvar apenas em trilho-zones (separado do wizard state)
            localStorage.setItem('trilho-zones', JSON.stringify(this.zones));
            console.log('✅ Zonas salvas no localStorage:', this.zones);
        } catch (error) {
            console.error('❌ Erro ao salvar zonas no localStorage:', error);
            if (error.name === 'QuotaExceededError') {
                console.log('🧹 Quota excedida para zonas, executando limpeza...');
                this.forceCleanupLocalStorage();
                // Tentar salvar novamente
                try {
                    localStorage.setItem('trilho-zones', JSON.stringify(this.zones));
                    console.log('✅ Zonas salvas após limpeza');
                } catch (retryError) {
                    console.error('❌ Ainda não foi possível salvar zonas:', retryError);
                }
            }
        }
    }

    setupZonesTVSimulation(containerWidth, containerHeight, trilhoMin, trilhoMax) {
        console.log('🔧 === SETUP TV SIMULATION ===');
        console.log('Container:', containerWidth, 'x', containerHeight);
        console.log('Trilho:', trilhoMin, 'a', trilhoMax);
        
        const tvOverlay = document.getElementById('zones-simulation-tv-overlay');
        if (!tvOverlay) {
            console.log('❌ TV simulation overlay não encontrado');
            console.log('🔍 Procurando elementos com ID zones-simulation-tv-overlay...');
            const allElements = document.querySelectorAll('[id*="simulation-tv"]');
            console.log('Elementos encontrados:', allElements);
            return;
        }
        
        console.log('✅ TV simulation overlay encontrado:', tvOverlay);

        // Configurar posição inicial da TV
        const currentPosition = this.config.trilho.currentPositionCm || 150;
        const positionPercent = ((currentPosition - trilhoMin) / (trilhoMax - trilhoMin)) * 100;
        
        // Calcular dimensões da TV baseadas no container
        const tvWidth = Math.min(containerWidth * 0.25, 200); // 25% da largura ou máximo 200px
        const tvHeight = tvWidth * 1.5; // Proporção ajustada para incluir controles
        
        // Posicionar TV sobre a imagem de background
        tvOverlay.style.width = `${tvWidth}px`;
        tvOverlay.style.height = `${tvHeight}px`;
        tvOverlay.style.left = `${positionPercent}%`;
        tvOverlay.style.top = '50%';
        tvOverlay.style.transform = 'translate(-50%, -50%)';
        tvOverlay.style.position = 'absolute';
        tvOverlay.style.zIndex = '30'; // Z-index maior para ficar sobre as zonas
        tvOverlay.style.display = 'flex'; // Garantir que está visível
        
        // Garantir que a TV está dentro do container de background
        const backgroundContainer = document.getElementById('zones-background-container');
        if (backgroundContainer) {
            backgroundContainer.appendChild(tvOverlay);
        }
        
        // Configurar controles da TV
        this.setupTVControls();
        
        // Configurar drag & drop para a TV
        this.setupTVDragDrop(tvOverlay, trilhoMin, trilhoMax);
        
        console.log(`📺 TV simulation configurada: ${tvWidth}x${tvHeight}px na posição ${positionPercent}%`);
        console.log(`📺 TV overlay element:`, tvOverlay);
    }

    setupTVControls() {
        const moveLeftBtn = document.getElementById('tv-move-left');
        const moveRightBtn = document.getElementById('tv-move-right');
        const powerBtn = document.getElementById('tv-power-btn');
        const volumeUpBtn = document.getElementById('tv-volume-up');
        const volumeDownBtn = document.getElementById('tv-volume-down');

        if (moveLeftBtn) {
            moveLeftBtn.addEventListener('click', () => {
                console.log('◀ Mover TV para esquerda');
                this.moveTVLeft();
            });
        }

        if (moveRightBtn) {
            moveRightBtn.addEventListener('click', () => {
                console.log('▶ Mover TV para direita');
                this.moveTVRight();
            });
        }

        if (powerBtn) {
            powerBtn.addEventListener('click', () => {
                console.log('🔌 Botão Power clicado');
                // Implementar lógica de ligar/desligar TV
            });
        }

        if (volumeUpBtn) {
            volumeUpBtn.addEventListener('click', () => {
                console.log('🔊 Volume Up clicado');
                // Implementar lógica de aumentar volume
            });
        }

        if (volumeDownBtn) {
            volumeDownBtn.addEventListener('click', () => {
                console.log('🔉 Volume Down clicado');
                // Implementar lógica de diminuir volume
            });
        }
    }

    moveTVLeft() {
        const currentPosition = this.config.trilho.currentPositionCm || 150;
        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const tvWidth = this.config.trilho.screenWidthCm || 52.5;
        
        // Mover 10cm para a esquerda
        const newPosition = Math.max(trilhoMin, currentPosition - 10);
        
        console.log(`📺 Movendo TV para esquerda: ${currentPosition}cm → ${newPosition}cm`);
        this.updateTVPosition(newPosition);
    }

    moveTVRight() {
        const currentPosition = this.config.trilho.currentPositionCm || 150;
        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const tvWidth = this.config.trilho.screenWidthCm || 52.5;
        
        // Mover 10cm para a direita
        const newPosition = Math.min(trilhoMax - tvWidth, currentPosition + 10);
        
        console.log(`📺 Movendo TV para direita: ${currentPosition}cm → ${newPosition}cm`);
        this.updateTVPosition(newPosition);
    }

    setupTVDragDrop(tvOverlay, trilhoMin, trilhoMax) {
        let isDragging = false;
        let startX = 0;
        let startLeft = 0;

        tvOverlay.addEventListener('mousedown', (e) => {
            isDragging = true;
            startX = e.clientX;
            startLeft = parseFloat(tvOverlay.style.left) || 50; // 50% como padrão
            
            tvOverlay.style.cursor = 'grabbing';
            tvOverlay.style.zIndex = '200'; // Z-index ainda maior durante o drag
            
            e.preventDefault();
            console.log('📺 Iniciando drag da TV');
        });

        document.addEventListener('mousemove', (e) => {
            if (!isDragging) return;

            const deltaX = e.clientX - startX;
            const container = document.getElementById('zones-background-container');
            if (!container) return;

            const containerWidth = container.offsetWidth;
            const tvWidth = tvOverlay.offsetWidth;
            
            // Calcular nova posição em pixels
            const newLeftPx = startLeft * containerWidth / 100 + deltaX;
            const newLeftPercent = (newLeftPx / containerWidth) * 100;
            
            // Limitar dentro dos limites do container
            const minLeft = (tvWidth / 2 / containerWidth) * 100;
            const maxLeft = 100 - (tvWidth / 2 / containerWidth) * 100;
            const clampedLeft = Math.max(minLeft, Math.min(maxLeft, newLeftPercent));
            
            tvOverlay.style.left = `${clampedLeft}%`;
        });

        document.addEventListener('mouseup', () => {
            if (!isDragging) return;
            
            isDragging = false;
            tvOverlay.style.cursor = 'grab';
            tvOverlay.style.zIndex = '100'; // Voltar ao z-index normal
            
            // Calcular posição final em cm
            const finalLeftPercent = parseFloat(tvOverlay.style.left);
            const trilhoLength = trilhoMax - trilhoMin;
            const newPositionCm = trilhoMin + (finalLeftPercent / 100) * trilhoLength;
            
            console.log(`📺 TV movida para: ${newPositionCm.toFixed(1)}cm`);
            this.updateTVPosition(newPositionCm);
        });
    }

    setupMovableBullet(containerWidth, trilhoMin, trilhoMax) {
        const bullet = document.getElementById('zones-tv-bullet');
        if (!bullet) return;

        let isDragging = false;
        let startX = 0;
        let startLeft = 0;

        // Função para converter posição do bullet para centímetros
        const bulletToCm = (leftPercent) => {
            return trilhoMin + (leftPercent / 100) * (trilhoMax - trilhoMin);
        };

        // Função para converter centímetros para posição do bullet
        const cmToBullet = (cm) => {
            return ((cm - trilhoMin) / (trilhoMax - trilhoMin)) * 100;
        };

        // Event listeners para arrastar
        bullet.addEventListener('mousedown', (e) => {
            isDragging = true;
            startX = e.clientX;
            startLeft = parseFloat(bullet.style.left) || 50;
            bullet.style.cursor = 'grabbing';
            e.preventDefault();
        });

        document.addEventListener('mousemove', (e) => {
            if (!isDragging) return;

            const deltaX = e.clientX - startX;
            const trackWidth = containerWidth - 40; // Largura do trilho (container - padding)
            const deltaPercent = (deltaX / trackWidth) * 100;
            let newLeft = startLeft + deltaPercent;

            // Limitar entre 0% e 100%
            newLeft = Math.max(0, Math.min(100, newLeft));
            bullet.style.left = `${newLeft}%`;

            // Atualizar posição da TV
            const tvCm = bulletToCm(newLeft);
            this.updateTVPosition(tvCm);
        });

        document.addEventListener('mouseup', () => {
            if (isDragging) {
                isDragging = false;
                bullet.style.cursor = 'pointer';
            }
        });

        // Posicionar bullet inicialmente na posição configurada
        const currentPosition = this.config.trilho.currentPositionCm || 150;
        const initialPosition = cmToBullet(currentPosition);
        bullet.style.left = `${initialPosition}%`;
        console.log(`📺 Posição inicial da TV: ${currentPosition}cm (${initialPosition}%)`);
    }

    updateTVPosition(positionCm) {
        const trilhoMin = this.config.trilho.physicalMinCm || 0;
        const trilhoMax = this.config.trilho.physicalMaxCm || 300;
        const trilhoLength = trilhoMax - trilhoMin;
        const positionPercent = ((positionCm - trilhoMin) / trilhoLength) * 100;

        // TV overlay removido (usando apenas zones-simulation-tv-overlay)

        // Atualizar bullet do background (sobre a imagem)
        const backgroundBullet = document.getElementById('zones-tv-bullet');
        if (backgroundBullet) {
            backgroundBullet.style.left = `${positionPercent}%`;
            backgroundBullet.style.top = '50%';
            backgroundBullet.style.transform = 'translateY(-50%)';
        }

        // Atualizar bullet do trilho
        const trilhoBullet = document.getElementById('trilho-tv-bullet');
        if (trilhoBullet) {
            trilhoBullet.style.left = `${positionPercent}%`;
        }

        // Atualizar TV simulation overlay
        const tvSimulationOverlay = document.getElementById('zones-simulation-tv-overlay');
        if (tvSimulationOverlay) {
            tvSimulationOverlay.style.left = `${positionPercent}%`;
        }

        // Atualizar configuração
        this.config.trilho.currentPositionCm = positionCm;
        console.log(`📺 Posição da TV atualizada: ${positionCm}cm`);
    }
    
    createFloorDistanceLine(bgContainer, scale) {
        // Calcular altura da TV do piso em pixels
        const tvHeightFromFloor = this.config.trilho.tvHeightFromFloor || 80;
        const bgHeight = this.config.background.heightCm || 200;
        
        // Calcular escala da simulação
        const maxWidth = 600;
        const bgSimHeight = Math.min(bgHeight * scale, maxWidth);
        
        // Calcular posição da linha do piso
        const floorPosition = (tvHeightFromFloor / bgHeight) * bgSimHeight;
        
        // Criar linha do piso (base da TV)
        const floorLine = document.createElement('div');
        floorLine.id = 'simulation-floor-line';
        floorLine.style.cssText = `
            position: absolute;
            bottom: ${floorPosition}px;
            left: 0;
            right: 0;
            height: 3px;
            background: #dc2626;
            z-index: 5;
            box-shadow: 0 0 5px rgba(220, 38, 38, 0.5);
        `;
        
        // Criar label da distância
        const distanceLabel = document.createElement('div');
        distanceLabel.id = 'simulation-distance-label';
        distanceLabel.style.cssText = `
            position: absolute;
            left: 10px;
            bottom: ${floorPosition + 10}px;
            background: rgba(220, 38, 38, 0.9);
            color: white;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
            z-index: 10;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
        `;
        distanceLabel.textContent = `${tvHeightFromFloor}cm`;
        
        // Criar indicador de posição da TV
        const positionIndicator = document.createElement('div');
        positionIndicator.id = 'simulation-position-indicator';
        positionIndicator.style.cssText = `
            position: absolute;
            right: 10px;
            bottom: ${floorPosition + 10}px;
            background: rgba(59, 130, 246, 0.9);
            color: white;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
            z-index: 10;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
        `;
        positionIndicator.textContent = 'Posição: 0cm';
        
        bgContainer.appendChild(floorLine);
        bgContainer.appendChild(distanceLabel);
        bgContainer.appendChild(positionIndicator);
        
        console.log(`✅ Linha de distância criada - Posição: ${floorPosition}px, Altura do piso: ${tvHeightFromFloor}cm`);
    }
    
    createTrackSimulation(bgContainer, scale) {
        // Função removida - não precisamos mais da barra com seta
        // Configurar controles manuais diretamente
        this.setupManualControls(bgContainer, scale);
        
        console.log('✅ Controles de movimento configurados');
    }
    
    setupManualControls(bgContainer, scale) {
        // Obter dimensões do background
        const bgWidth = this.config.background.widthCm || 300;
        const bgHeight = this.config.background.heightCm || 200;
        
        // Calcular escala da simulação
        const maxWidth = 600;
        const bgSimWidth = Math.min(bgWidth * scale, maxWidth);
        const bgSimHeight = Math.min(bgHeight * scale, maxWidth);
        
        // Calcular dimensões da TV
        const tvWidthCm = 52.5;
        const tvSimWidth = (tvWidthCm / bgWidth) * bgSimWidth;
        
        // Calcular movimento de 10cm por vez
        const stepPixels = (10 / bgWidth) * bgSimWidth; // 10cm em pixels
        
        // Calcular limites de movimento da TV
        const startX = tvSimWidth / 2; // Posição inicial (0cm)
        const maxLeft = bgSimWidth - (tvSimWidth / 2); // Posição máxima
        const minLeft = tvSimWidth / 2; // Posição mínima (0cm)
        
        console.log(`Limites da TV: minLeft=${minLeft}px, maxLeft=${maxLeft}px, step=${stepPixels.toFixed(1)}px (10cm)`);
        
        // Função para mover TV com interpolação suave
        const moveTV = (direction) => {
            const tvOverlay = document.getElementById('simulation-tv-overlay');
            if (tvOverlay) {
                const currentLeft = parseFloat(tvOverlay.style.left) || startX;
                let targetLeft;
                
                if (direction === 'left') {
                    targetLeft = Math.max(minLeft, currentLeft - stepPixels);
                } else if (direction === 'right') {
                    targetLeft = Math.min(maxLeft, currentLeft + stepPixels);
                }
                
                // Interpolação suave
                this.animateTVMovement(tvOverlay, currentLeft, targetLeft, startX, bgWidth);
                
                console.log(`TV movendo para ${direction}: ${targetLeft}px (10cm)`);
            }
        };
        
        // Função para resetar TV com animação suave
        const resetTV = () => {
            const tvOverlay = document.getElementById('simulation-tv-overlay');
            if (tvOverlay) {
                const initialLeft = tvOverlay.dataset.initialLeft;
                let resetX;
                
                if (initialLeft) {
                    resetX = parseFloat(initialLeft);
                } else {
                    resetX = startX; // Posição inicial (0cm)
                }
                
                const currentLeft = parseFloat(tvOverlay.style.left) || startX;
                
                // Usar animação suave para reset
                this.animateTVMovement(tvOverlay, currentLeft, resetX, startX, bgWidth);
                
                console.log(`TV resetando para posição inicial: ${resetX}px (0cm)`);
            }
        };
        
        // Configurar botões
        const leftBtn = document.getElementById('simulation-left');
        const rightBtn = document.getElementById('simulation-right');
        const resetBtn = document.getElementById('simulation-reset');
        
        console.log('🔍 Procurando botões:', {
            leftBtn: !!leftBtn,
            rightBtn: !!rightBtn,
            resetBtn: !!resetBtn
        });
        
        if (leftBtn) {
            leftBtn.onclick = () => {
                console.log('🔄 Botão esquerda clicado');
                moveTV('left');
            };
            console.log('✅ Botão esquerda configurado');
        } else {
            console.error('❌ Botão esquerda não encontrado');
        }
        
        if (rightBtn) {
            rightBtn.onclick = () => {
                console.log('🔄 Botão direita clicado');
                moveTV('right');
            };
            console.log('✅ Botão direita configurado');
        } else {
            console.error('❌ Botão direita não encontrado');
        }
        
        if (resetBtn) {
            resetBtn.onclick = () => {
                console.log('🔄 Botão reset clicado');
                resetTV();
            };
            console.log('✅ Botão reset configurado');
        } else {
            console.error('❌ Botão reset não encontrado');
        }
        
        console.log('✅ Controles manuais da TV configurados');
    }
    
    updatePositionIndicator(tvLeft, startX, bgWidth) {
        // Calcular posição da TV em centímetros
        const positionIndicator = document.getElementById('simulation-position-indicator');
        if (!positionIndicator) return;
        
        // Calcular deslocamento da TV em relação ao lado esquerdo (0cm)
        const offset = tvLeft - startX;
        
        // Converter offset em centímetros
        // 0cm = lado esquerdo alinhado
        // Positivo = movimento para a direita
        const pixelsPerCm = 600 / bgWidth; // 600px é a largura máxima da simulação
        const positionCm = offset / pixelsPerCm;
        
        // Atualizar indicador
        positionIndicator.textContent = `Posição: ${positionCm.toFixed(1)}cm`;
        
        console.log(`📍 Posição atualizada: ${positionCm.toFixed(1)}cm (offset: ${offset.toFixed(1)}px)`);
    }
    
    animateTVMovement(tvOverlay, startPos, endPos, startX, bgWidth) {
        // Interpolação suave da TV
        const duration = 300; // 300ms de animação
        const startTime = performance.now();
        
        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            // Função de easing (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);
            
            // Calcular posição interpolada
            const currentPos = startPos + (endPos - startPos) * easeOut;
            
            // Aplicar posição
            tvOverlay.style.left = `${currentPos}px`;
            tvOverlay.style.transform = 'translateX(-50%)';
            
            // Atualizar indicador de posição
            this.updatePositionIndicator(currentPos, startX, bgWidth);
            
            // Continuar animação se não terminou
            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                console.log(`✅ Animação concluída: ${endPos}px`);
            }
        };
        
        requestAnimationFrame(animate);
    }
    
    createTVOverlay(bgContainer, scale) {
        // Dimensões da TV 42" em portrait
        const tvWidthCm = 52.5;
        const tvHeightCm = 93.5;
        
        // Dimensões do background
        const bgWidth = this.config.background.widthCm || 300;
        const bgHeight = this.config.background.heightCm || 200;
        
        // Calcular escala da simulação
        const maxWidth = 600;
        const bgSimWidth = Math.min(bgWidth * scale, maxWidth);
        const bgSimHeight = Math.min(bgHeight * scale, maxWidth);
        
        // Calcular dimensões da TV em pixels
        const tvSimWidth = (tvWidthCm / bgWidth) * bgSimWidth;
        const tvSimHeight = (tvHeightCm / bgHeight) * bgSimHeight;
        
        // NOVA ABORDAGEM: Calcular posição da TV de forma mais simples
        const tvHeightFromFloor = this.config.trilho.tvHeightFromFloor || 80;
        
        // Calcular a posição do topo da TV
        // Se a TV tem 93.5cm de altura e está a 80cm do piso
        // O topo da TV está a 80cm + 93.5cm = 173.5cm do piso
        const tvTopFromFloor = tvHeightFromFloor + tvHeightCm; // 80 + 93.5 = 173.5cm
        const tvTopPosition = ((bgHeight - tvTopFromFloor) / bgHeight) * bgSimHeight;
        
        // Posição inicial da TV (lado esquerdo = 0cm)
        const startX = tvSimWidth / 2;
        
        console.log(`📐 TV: ${tvSimWidth.toFixed(1)}x${tvSimHeight.toFixed(1)}px`);
        console.log(`📐 TV Top from floor: ${tvTopFromFloor}cm`);
        console.log(`📐 TV Top position: ${tvTopPosition.toFixed(1)}px`);
        console.log(`📐 StartX: ${startX.toFixed(1)}px`);
        
        // Criar overlay da TV
        const tvOverlay = document.createElement('div');
        tvOverlay.id = 'simulation-tv-overlay';
        tvOverlay.className = 'simulation-tv-overlay';
        
        // Usar top para posicionar a TV
        tvOverlay.style.cssText = `
            position: absolute;
            width: ${tvSimWidth}px;
            height: ${tvSimHeight}px;
            border: 3px solid #2563eb;
            background-color: rgba(37, 99, 235, 0.1);
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 14px;
            color: #2563eb;
            font-weight: bold;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            top: ${tvTopPosition}px;
            left: ${startX}px;
            transform: translateX(-50%);
            z-index: 10;
            transition: left 0.3s ease;
            backdrop-filter: blur(0px) brightness(1.2) contrast(1.1);
            -webkit-backdrop-filter: blur(0px) brightness(1.2) contrast(1.1);
            border-radius: 8px;
            overflow: hidden;
        `;
        
        tvOverlay.innerHTML = `
            <div class="simulation-tv-content">
                <span>TV</span>
                <div class="magnifying-glass-effect"></div>
            </div>
        `;
        bgContainer.appendChild(tvOverlay);
        
        // Armazenar posição inicial
        tvOverlay.dataset.initialLeft = startX.toString();
        
        // Inicializar indicador de posição
        this.updatePositionIndicator(startX, startX, bgWidth);
        
        // Adicionar efeito de lupa ativo por padrão
        tvOverlay.classList.add('magnifying-active');
        
        console.log(`✅ TV criada - Top: ${tvTopPosition.toFixed(1)}px, Left: ${startX.toFixed(1)}px`);
    }


    checkBackgroundDimensionsReady() {
        const width = parseFloat(document.getElementById('background-width-cm')?.value || 0);
        const height = parseFloat(document.getElementById('background-height-cm')?.value || 0);
        
        const uploadGroup = document.getElementById('background-upload-group');
        const dimensionsAlert = document.getElementById('background-dimensions-alert');
        
        console.log('🔍 Verificando dimensões do background:', { width, height });
        console.log('🔍 Elementos encontrados:', { uploadGroup: !!uploadGroup, dimensionsAlert: !!dimensionsAlert });
        
        if (width > 0 && height > 0) {
            // Dimensões configuradas - habilitar upload
            if (uploadGroup) {
                uploadGroup.style.display = 'block';
                console.log('✅ Upload habilitado - grupo visível');
            }
            if (dimensionsAlert) {
                dimensionsAlert.style.display = 'none';
                console.log('✅ Alerta oculto');
            }
            console.log('✅ Upload de imagem habilitado - dimensões configuradas');
        } else {
            // Dimensões não configuradas - desabilitar upload
            if (uploadGroup) {
                uploadGroup.style.display = 'none';
                console.log('❌ Upload desabilitado - grupo oculto');
            }
            if (dimensionsAlert) {
                dimensionsAlert.style.display = 'block';
                console.log('❌ Alerta visível');
            }
            console.log('❌ Upload de imagem desabilitado - configure as dimensões primeiro');
        }
    }

    simulateMovement() {
        console.log('Função de simulação automática removida - use os botões de controle manual');
    }

    resetSimulation() {
        console.log('Resetando simulação...');
        
        // Resetar posição da TV
        const tvOverlay = document.getElementById('simulation-tv-overlay');
        if (tvOverlay) {
            // Usar posição inicial armazenada ou calcular
            const initialLeft = tvOverlay.dataset.initialLeft;
            let centerX;
            
            if (initialLeft) {
                centerX = parseFloat(initialLeft);
            } else {
                // Calcular posição central em pixels
                const bgWidth = this.config.background.widthCm || 300;
                const maxWidth = 600;
                const scale = Math.min(maxWidth / bgWidth, 1);
                const bgSimWidth = Math.min(bgWidth * scale, maxWidth);
                centerX = bgSimWidth / 2;
            }
            
            tvOverlay.style.left = `${centerX}px`;
            tvOverlay.style.transform = 'translateX(-50%)';
            tvOverlay.style.transition = 'left 0.3s ease';
            console.log(`TV resetada para posição central: ${centerX}px`);
        }
    }

    updateURL() {
        const stepNames = {
            1: 'projeto',
            2: 'trilho', 
            3: 'osc',
            4: 'background',
            5: 'zonas',
            6: 'exportar'
        };
        
        const stepName = stepNames[this.currentStep] || 'projeto';
        const newURL = `${window.location.pathname}#${stepName}`;
        
        if (window.location.href !== newURL) {
            window.history.pushState({ step: this.currentStep }, '', newURL);
            console.log(`URL atualizada para: ${newURL}`);
        }
    }

    loadStepFromURL() {
        const hash = window.location.hash.substring(1); // Remove #
        const stepNames = {
            'projeto': 1,
            'trilho': 2,
            'osc': 3,
            'background': 4,
            'zonas': 5,
            'exportar': 6
        };
        
        const stepFromURL = stepNames[hash];
        if (stepFromURL && stepFromURL !== this.currentStep) {
            console.log(`Carregando step ${stepFromURL} da URL: ${hash}`);
            this.currentStep = stepFromURL;
        }
    }

    initializeWizard() {
        console.log('Inicializando wizard...');
        
        this.currentStep = 1;
        this.totalSteps = 6;
        this.completedSteps = new Set();
        
        // Event listeners para navegação
        document.getElementById('wizard-prev').addEventListener('click', () => this.previousStep());
        document.getElementById('wizard-next').addEventListener('click', () => this.nextStep());
        document.getElementById('wizard-finish').addEventListener('click', () => this.finishWizard());
        
        // Event listeners para cliques nos steps
        document.querySelectorAll('.wizard-step').forEach(step => {
            step.addEventListener('click', (e) => {
                const stepNumber = parseInt(e.currentTarget.dataset.step);
                this.goToStep(stepNumber);
            });
        });
        
        // Autosave a cada mudança
        this.setupAutosave();
        
        // Carregar estado salvo
        this.loadWizardState();
        
        // Check URL hash for initial step
        this.loadStepFromURL();
        
        // Atualizar UI inicial
        this.updateWizardUI();
        
        // Listen for browser back/forward
        window.addEventListener('popstate', (event) => {
            if (event.state && event.state.step) {
                this.currentStep = event.state.step;
                this.updateWizardUI();
            } else {
                this.loadStepFromURL();
                this.updateWizardUI();
            }
        });
    }

    setupAutosave() {
        // Autosave a cada mudança nos campos
        document.addEventListener('input', () => {
            this.saveToLocalStorage();
        });
        
        document.addEventListener('change', () => {
            this.saveToLocalStorage();
        });
        
        // Autosave periódico a cada 30 segundos
        setInterval(() => {
            this.saveToLocalStorage();
        }, 30000);
    }

    saveToLocalStorage() {
        try {
            // Limpar dados desnecessários antes de salvar
            this.cleanupLocalStorage();
            
            // Criar uma cópia da config sem o uploadedFile (não pode ser serializado)
            const configToSave = { ...this.config };
            if (configToSave.background && configToSave.background.uploadedFile) {
                // Manter apenas o nome do arquivo, não o objeto File
                configToSave.background = {
                    ...configToSave.background,
                    uploadedFile: null,
                    hasUploadedFile: true,
                    imageFile: this.config.background.imageFile || this.config.background.uploadedFile?.name || ''
                };
            }
            
            const wizardState = {
                currentStep: this.currentStep,
                completedSteps: Array.from(this.completedSteps),
                config: configToSave,
                // zones: this.zones, // Removido - salvo separadamente em trilho-zones
                lastSaved: new Date().toISOString()
            };
            
            localStorage.setItem('trilho-wizard-state', JSON.stringify(wizardState));
            console.log('Estado do wizard salvo no localStorage');
        } catch (error) {
            console.error('Erro ao salvar no localStorage:', error);
            if (error.name === 'QuotaExceededError') {
                console.log('🧹 Quota excedida, executando limpeza forçada...');
                this.forceCleanupLocalStorage();
                // Tentar salvar novamente após limpeza
                try {
                    const configToSave = { ...this.config };
                    if (configToSave.background && configToSave.background.uploadedFile) {
                        configToSave.background = {
                            ...configToSave.background,
                            uploadedFile: null,
                            hasUploadedFile: true,
                            imageFile: this.config.background.imageFile || this.config.background.uploadedFile?.name || ''
                        };
                    }
                    
                    const wizardState = {
                        currentStep: this.currentStep,
                        completedSteps: Array.from(this.completedSteps),
                        config: configToSave,
                        zones: this.zones,
                        lastSaved: new Date().toISOString()
                    };
                    
                    localStorage.setItem('trilho-wizard-state', JSON.stringify(wizardState));
                    console.log('✅ Dados salvos após limpeza forçada');
                } catch (retryError) {
                    console.error('❌ Ainda não foi possível salvar após limpeza:', retryError);
                    this.showStorageFullDialog();
                }
            }
        }
    }

    cleanupLocalStorage() {
        // Remover dados antigos ou desnecessários
        const keysToRemove = [];
        
        // Verificar todas as chaves do localStorage
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && key.startsWith('trilho-')) {
                // Manter apenas as chaves essenciais
                if (!['trilho-wizard-state', 'trilho-zones', 'trilho-background-image'].includes(key)) {
                    keysToRemove.push(key);
                }
            }
        }
        
        // Remover chaves desnecessárias
        keysToRemove.forEach(key => {
            localStorage.removeItem(key);
            console.log(`🗑️ Removido: ${key}`);
        });
        
        if (keysToRemove.length > 0) {
            console.log(`🧹 Limpeza concluída: ${keysToRemove.length} itens removidos`);
        }
    }

    forceCleanupLocalStorage() {
        // Limpeza mais agressiva em caso de quota excedida
        console.log('🧹 Executando limpeza forçada do localStorage...');
        
        // Remover dados de background antigos (podem ser grandes)
        const backgroundKeys = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && (key.includes('background') || key.includes('image'))) {
                backgroundKeys.push(key);
            }
        }
        
        // Manter apenas o background mais recente
        if (backgroundKeys.length > 1) {
            backgroundKeys.slice(0, -1).forEach(key => {
                localStorage.removeItem(key);
                console.log(`🗑️ Removido background antigo: ${key}`);
            });
        }
        
        // Limpar dados de zonas com vídeos extremamente grandes (apenas acima de 50MB)
        const zones = JSON.parse(localStorage.getItem('trilho-zones') || '[]');
        const cleanedZones = zones.map(zone => {
            // Remover apenas vídeos extremamente grandes (acima de 50MB)
            if (zone.videoData && zone.videoData.length > 50000000) {
                console.log(`🗑️ Removendo vídeo extremamente grande da zona: ${zone.name} (${(zone.videoData.length / 1024 / 1024).toFixed(1)}MB)`);
                zone.videoData = null;
                zone.type = 'text'; // Converter para texto se não tiver imagem
                zone.text = zone.text || 'Vídeo removido por ser extremamente grande';
            }
            return zone;
        });
        
        if (zones.length !== cleanedZones.length || zones.some((zone, i) => zone.videoData !== cleanedZones[i].videoData)) {
            localStorage.setItem('trilho-zones', JSON.stringify(cleanedZones));
            console.log('✂️ Dados de vídeo otimizados');
        }
        
        // Limpar dados de configuração antigos
        const configKeys = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && key.includes('config') && key !== 'trilho-wizard-state') {
                configKeys.push(key);
            }
        }
        
        configKeys.forEach(key => {
            localStorage.removeItem(key);
            console.log(`🗑️ Removido config antigo: ${key}`);
        });
        
        // Limpar trilho-wizard-state se muito grande
        try {
            const currentState = localStorage.getItem('trilho-wizard-state');
            if (currentState && currentState.length > 2 * 1024 * 1024) { // 2MB
                console.log('🗑️ trilho-wizard-state muito grande, limpando...');
                localStorage.removeItem('trilho-wizard-state');
            }
        } catch (error) {
            console.log('Erro ao verificar tamanho do wizard state:', error);
        }
    }

    checkLocalStorageUsage() {
        let totalSize = 0;
        const items = [];
        
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            const value = localStorage.getItem(key);
            const size = new Blob([value]).size;
            totalSize += size;
            items.push({ key, size });
        }
        
        const totalSizeMB = (totalSize / (1024 * 1024)).toFixed(2);
        console.log(`📊 Uso do localStorage: ${totalSizeMB}MB`);
        
        // Mostrar os maiores itens
        items.sort((a, b) => b.size - a.size);
        console.log('📋 Maiores itens no localStorage:');
        items.slice(0, 5).forEach(item => {
            const sizeMB = (item.size / (1024 * 1024)).toFixed(2);
            console.log(`  - ${item.key}: ${sizeMB}MB`);
        });
        
        return { totalSize, totalSizeMB, items };
    }

    showStorageFullDialog() {
        // Criar modal de armazenamento cheio
        const modal = document.createElement('div');
        modal.className = 'modal-overlay';
        modal.style.zIndex = '10000';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 500px;">
                <div class="modal-header">
                    <h3>⚠️ Armazenamento Cheio</h3>
                    <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="storage-warning">
                        <p><strong>⚠️ O armazenamento local está cheio!</strong></p>
                        <p>Isso geralmente acontece quando há vídeos muito grandes salvos. Para continuar usando o configurador, você precisa limpar alguns dados.</p>
                    </div>
                    <div class="storage-info">
                        <h4>Opções disponíveis:</h4>
                        <div class="storage-options">
                            <button class="btn btn-warning" onclick="window.trilhoConfigurator.clearAllData()">
                                🗑️ Limpar Todos os Dados
                            </button>
                            <button class="btn btn-secondary" onclick="window.trilhoConfigurator.clearOldData()">
                                🧹 Limpar Dados Antigos
                            </button>
                            <button class="btn btn-info" onclick="window.trilhoConfigurator.showStorageDetails()">
                                📊 Ver Detalhes
                            </button>
                        </div>
                    </div>
                    <div class="storage-tip">
                        <p><strong>💡 Dica:</strong> Arquivos grandes podem ocupar muito espaço. Use o botão "Limpar Dados Antigos" para liberar espaço automaticamente.</p>
                    </div>
                    <div class="storage-warning">
                        <small>⚠️ <strong>Limpar Todos os Dados</strong> irá remover todas as configurações e zonas salvas.</small>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-primary" onclick="this.closest('.modal-overlay').remove()">
                        Fechar
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Adicionar estilos se não existirem
        if (!document.getElementById('storage-modal-styles')) {
            const style = document.createElement('style');
            style.id = 'storage-modal-styles';
            style.textContent = `
                .storage-info {
                    margin: 1rem 0;
                }
                .storage-options {
                    display: flex;
                    flex-direction: column;
                    gap: 0.5rem;
                    margin: 1rem 0;
                }
                .storage-options .btn {
                    padding: 0.75rem;
                    text-align: left;
                }
                .storage-warning {
                    background: #fff3cd;
                    border: 1px solid #ffeaa7;
                    border-radius: 4px;
                    padding: 0.75rem;
                    margin-top: 1rem;
                }
            `;
            document.head.appendChild(style);
        }
    }

    clearAllData() {
        if (confirm('⚠️ Tem certeza que deseja limpar TODOS os dados? Esta ação não pode ser desfeita.')) {
            localStorage.clear();
            console.log('🗑️ Todos os dados do localStorage foram removidos');
            this.showToast('Todos os dados foram limpos. Recarregue a página.', 'success');
            
            // Fechar modal
            const modal = document.querySelector('.modal-overlay');
            if (modal) modal.remove();
            
            // Recarregar página após 2 segundos
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    }

    clearOldData() {
        console.log('🧹 Limpando dados antigos...');
        
        // Limpeza mais agressiva
        this.forceCleanupLocalStorage();
        
        // Remover dados de wizard antigos
        const wizardKeys = [];
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key && key.includes('wizard') && key !== 'trilho-wizard-state') {
                wizardKeys.push(key);
            }
        }
        
        wizardKeys.forEach(key => {
            localStorage.removeItem(key);
            console.log(`🗑️ Removido wizard antigo: ${key}`);
        });
        
        // Tentar salvar novamente
        try {
            this.saveToLocalStorage();
            this.showToast('Dados antigos removidos com sucesso!', 'success');
            
            // Fechar modal
            const modal = document.querySelector('.modal-overlay');
            if (modal) modal.remove();
        } catch (error) {
            console.error('❌ Ainda não foi possível salvar:', error);
            this.showToast('Ainda não foi possível salvar. Tente limpar todos os dados.', 'error');
        }
    }

    showStorageDetails() {
        const usage = this.checkLocalStorageUsage();
        
        // Criar modal de detalhes
        const modal = document.createElement('div');
        modal.className = 'modal-overlay';
        modal.style.zIndex = '10001';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 600px;">
                <div class="modal-header">
                    <h3>📊 Detalhes do Armazenamento</h3>
                    <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="storage-summary">
                        <h4>Uso Total: ${usage.totalSizeMB}MB</h4>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: ${Math.min(100, (usage.totalSize / (5 * 1024 * 1024)) * 100)}%"></div>
                        </div>
                        <small>Limite aproximado: 5MB</small>
                    </div>
                    <div class="storage-items">
                        <h4>Maiores Itens:</h4>
                        <div class="items-list">
                            ${usage.items.slice(0, 10).map(item => {
                                const sizeMB = (item.size / (1024 * 1024)).toFixed(2);
                                return `<div class="item-row">
                                    <span class="item-key">${item.key}</span>
                                    <span class="item-size">${sizeMB}MB</span>
                                </div>`;
                            }).join('')}
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-primary" onclick="this.closest('.modal-overlay').remove()">
                        Fechar
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Adicionar estilos para detalhes
        if (!document.getElementById('storage-details-styles')) {
            const style = document.createElement('style');
            style.id = 'storage-details-styles';
            style.textContent = `
                .storage-summary {
                    margin: 1rem 0;
                }
                .progress-bar {
                    width: 100%;
                    height: 20px;
                    background: #e0e0e0;
                    border-radius: 10px;
                    overflow: hidden;
                    margin: 0.5rem 0;
                }
                .progress-fill {
                    height: 100%;
                    background: linear-gradient(90deg, #4caf50, #ff9800, #f44336);
                    transition: width 0.3s ease;
                }
                .items-list {
                    max-height: 300px;
                    overflow-y: auto;
                }
                .item-row {
                    display: flex;
                    justify-content: space-between;
                    padding: 0.5rem;
                    border-bottom: 1px solid #eee;
                }
                .item-key {
                    font-family: monospace;
                    font-size: 0.9rem;
                }
                .item-size {
                    font-weight: bold;
                    color: #666;
                }
            `;
            document.head.appendChild(style);
        }
    }

    loadWizardState() {
        try {
            const savedState = localStorage.getItem('trilho-wizard-state');
            if (savedState) {
                const state = JSON.parse(savedState);
                this.currentStep = state.currentStep || 1;
                this.completedSteps = new Set(state.completedSteps || []);
                
                if (state.config) {
                    this.config = { ...this.config, ...state.config };
                    
                    // Se havia um arquivo carregado, limpar a referência (não pode ser restaurada)
                    if (this.config.background && this.config.background.imageFile) {
                        this.config.background.uploadedFile = null;
                        this.config.background.hasUploadedFile = true; // Manter como true para mostrar placeholder específico
                        console.log('Arquivo de background não pode ser restaurado do localStorage');
                    }
                }
                
                // Carregar zonas separadamente do trilho-zones
                this.loadZonesFromStorage();
                
                console.log('Estado do wizard carregado do localStorage');
                console.log('Step atual:', this.currentStep);
                console.log('Steps completados:', Array.from(this.completedSteps));
                
                // Popular campos do formulário com dados carregados
                this.populateFormFields();
                
                // Verificar se background está pronto
                this.checkBackgroundDimensionsReady();
            }
        } catch (error) {
            console.error('Erro ao carregar do localStorage:', error);
        }
    }

    goToStep(stepNumber) {
        if (stepNumber < 1 || stepNumber > this.totalSteps) return;
        
        // Validar se pode ir para o step
        if (stepNumber > 1 && !this.completedSteps.has(stepNumber - 1)) {
            this.showToast('Complete a etapa anterior primeiro', 'warning');
            return;
        }
        
        this.currentStep = stepNumber;
        this.updateWizardUI();
        this.updateURL();
        this.saveToLocalStorage();
    }

    nextStep() {
        // Validar etapa atual
        if (!this.validateCurrentStep()) {
            this.showToast('Complete os campos obrigatórios antes de continuar', 'warning');
            return;
        }
        
        // Marcar etapa como completa
        this.completedSteps.add(this.currentStep);
        
        // Ir para próxima etapa
        if (this.currentStep < this.totalSteps) {
            this.currentStep++;
            this.updateWizardUI();
            this.updateURL();
            
            // Salvar estado (zones são salvas separadamente)
            console.log('💾 Salvando estado antes de avançar - Zonas atuais:', this.zones);
            try {
                this.saveToLocalStorage();
            } catch (error) {
                console.log('⚠️ Erro ao salvar wizard state, mas zonas são salvas separadamente');
            }
        }
    }

    previousStep() {
        if (this.currentStep > 1) {
        this.currentStep--;
        this.updateWizardUI();
        this.updateURL();
        this.saveToLocalStorage();
        }
    }

    finishWizard() {
        // Validar todas as etapas
        if (!this.validateAllSteps()) {
            this.showToast('Complete todas as etapas antes de exportar', 'warning');
            return;
        }
        
        // Marcar todas como completas
        for (let i = 1; i <= this.totalSteps; i++) {
            this.completedSteps.add(i);
        }
        
        this.updateWizardUI();
        this.saveToLocalStorage();
        
        // Exportar configuração
        this.exportToUnity();
    }

    validateCurrentStep() {
        switch (this.currentStep) {
            case 1: // Projeto
                return this.validateProjectInfo();
            case 2: // Trilho
                return this.validateTrilhoConfig();
            case 3: // OSC
                return this.validateOSCConfig();
            case 4: // Background
                return this.validateBackgroundConfig();
            case 5: // Zonas
                return this.validateZonesConfig();
            case 6: // Exportar
                return true;
            default:
                return false;
        }
    }

    validateAllSteps() {
        for (let i = 1; i <= this.totalSteps; i++) {
            this.currentStep = i;
            if (!this.validateCurrentStep()) {
                return false;
            }
        }
        return true;
    }

    updateWizardUI() {
        // Atualizar steps
        document.querySelectorAll('.wizard-step').forEach((step, index) => {
            const stepNumber = index + 1;
            step.classList.remove('active', 'completed');
            
            if (stepNumber === this.currentStep) {
                step.classList.add('active');
            } else if (this.completedSteps.has(stepNumber)) {
                step.classList.add('completed');
            }
        });
        
        // Atualizar conteúdo das seções
        document.querySelectorAll('.wizard-step-content').forEach((content, index) => {
            const stepNumber = index + 1;
            content.classList.remove('active', 'completed');
            
            if (stepNumber === this.currentStep) {
                content.classList.add('active');
            } else if (this.completedSteps.has(stepNumber)) {
                content.classList.add('completed');
            }
        });
        
        // Chamar showSection para a seção ativa
        const sectionNames = ['general', 'trilho', 'osc', 'background', 'zones', 'export'];
        const activeSectionName = sectionNames[this.currentStep - 1];
        if (activeSectionName) {
            console.log(`🎯 Ativando seção via wizard: ${activeSectionName}`);
            this.showSection(activeSectionName);
        }
        
        // Atualizar botões de navegação
        const prevBtn = document.getElementById('wizard-prev');
        const nextBtn = document.getElementById('wizard-next');
        const finishBtn = document.getElementById('wizard-finish');
        
        if (prevBtn) {
            prevBtn.disabled = this.currentStep === 1;
        }
        
        if (nextBtn) {
            nextBtn.style.display = this.currentStep < this.totalSteps ? 'block' : 'none';
        }
        
        if (finishBtn) {
            finishBtn.style.display = this.currentStep === this.totalSteps ? 'block' : 'none';
        }
        
        // Atualizar título da página
        const stepTitles = ['Projeto', 'Trilho', 'OSC', 'Background', 'Zonas', 'Exportar'];
        document.title = `Trilho Configurador - ${stepTitles[this.currentStep - 1]}`;
        
        // Atualizar URL se necessário
        this.updateURL();
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
            
 else {
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
            
            // Mostrar seção de sistema após salvar
            this.showSystemInfo();
            
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
            // Verificar dados de imagem (Base64)
            if (zone.type === 'image' && zone.imageData) {
                const fileName = `${zone.name.replace(/[^a-zA-Z0-9]/g, '_')}.jpg`;
                const file = this.base64ToFile(zone.imageData, fileName, 'image/jpeg');
                mediaFiles.push({
                    name: fileName,
                    file: file,
                    type: 'image'
                });
            }
            
            // Verificar dados de vídeo (Base64)
            if (zone.type === 'video' && zone.videoData) {
                const fileName = `${zone.name.replace(/[^a-zA-Z0-9]/g, '_')}.mp4`;
                const file = this.base64ToFile(zone.videoData, fileName, 'video/mp4');
                mediaFiles.push({
                    name: fileName,
                    file: file,
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
    
    base64ToFile(base64Data, fileName, mimeType) {
        // Extrair os dados Base64 (remover o prefixo data:image/...;base64,)
        const base64 = base64Data.split(',')[1];
        
        // Converter para bytes
        const byteCharacters = atob(base64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        
        // Criar Blob e File
        const blob = new Blob([byteArray], { type: mimeType });
        return new File([blob], fileName, { type: mimeType });
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
        
        // Event fields
        const eventLocation = document.getElementById('event-location');
        if (eventLocation) eventLocation.value = this.config.project.eventLocation || '';
        
        const eventDate = document.getElementById('event-date');
        if (eventDate) eventDate.value = this.config.project.eventDate || '';
        
        const clientName = document.getElementById('client-name');
        if (clientName) clientName.value = this.config.project.clientName || '';
        
        const technicalResponsible = document.getElementById('technical-responsible');
        if (technicalResponsible) technicalResponsible.value = this.config.project.technicalResponsible || '';

        // Creation and modification dates
        const creationDate = document.getElementById('creation-date');
        if (creationDate) creationDate.value = this.config.creationDate ? new Date(this.config.creationDate).toISOString().slice(0, 16) : '';
        
        const lastModified = document.getElementById('last-modified');
        if (lastModified) lastModified.value = this.config.lastModified ? new Date(this.config.lastModified).toISOString().slice(0, 16) : '';

        // Trilho fields
        console.log('=== PREENCHENDO CAMPOS TRILHO ===');
        console.log('Configuração atual do trilho:', this.config.trilho);
        const trilhoFields = [
            { sliderId: 'physical-min-cm-slider', inputId: 'physical-min-cm', value: this.config.trilho.physicalMinCm },
            { sliderId: 'physical-max-cm-slider', inputId: 'physical-max-cm', value: this.config.trilho.physicalMaxCm },
            { sliderId: 'tv-height-from-floor-slider', inputId: 'tv-height-from-floor', value: this.config.trilho.tvHeightFromFloor },
            { sliderId: 'movement-sensitivity-slider', inputId: 'movement-sensitivity', value: this.config.trilho.movementSensitivity },
            { sliderId: 'camera-smoothing-slider', inputId: 'camera-smoothing', value: this.config.trilho.cameraSmoothing },
            { sliderId: 'pre-activation-range-slider', inputId: 'pre-activation-range', value: this.config.trilho.preActivationRange }
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

        // TV fields
        console.log('=== PREENCHENDO CAMPOS TV ===');
        const tvModel = document.getElementById('tv-model');
        const tvOrientation = document.getElementById('tv-orientation');
        const screenResolution = document.getElementById('screen-resolution');
        
        if (tvModel) tvModel.value = this.config.tv.model || '42';
        if (tvOrientation) tvOrientation.value = this.config.tv.orientation || 'portrait';
        if (screenResolution) screenResolution.value = this.config.tv.resolution || '1920x1080';
        
        console.log('TV fields definidos:', {
            model: this.config.tv.model,
            orientation: this.config.tv.orientation,
            resolution: this.config.tv.resolution
        });


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
        
        const backgroundFileName = document.getElementById('background-file-name');
        if (backgroundFileName) {
            if (this.config.background.hasUploadedFile && !this.config.background.uploadedFile) {
                backgroundFileName.textContent = `${this.config.background.imageFile} (recarregar)`;
            } else {
                backgroundFileName.textContent = this.config.background.imageFile || 'Nenhum arquivo selecionado';
            }
        }
        
        // Background dimensions
        const backgroundWidth = document.getElementById('background-width-cm');
        const backgroundHeight = document.getElementById('background-height-cm');
        if (backgroundWidth) {
            const widthValue = this.config.background.widthCm || 300;
            backgroundWidth.value = parseFloat(widthValue.toFixed(1));
        }
        if (backgroundHeight) {
            const heightValue = this.config.background.heightCm || 200;
            backgroundHeight.value = parseFloat(heightValue.toFixed(1));
        }
        
        console.log('Background fields definidos:', {
            enabled: this.config.background.enabled,
            imageFile: this.config.background.imageFile,
            widthCm: this.config.background.widthCm,
            heightCm: this.config.background.heightCm
        });
        
        console.log('=== CAMPOS POPULADOS ===');
    }

    renderZones() {
        const zonesContainer = document.getElementById('zones-container');
        if (zonesContainer) {
            zonesContainer.innerHTML = '';
            this.zones.forEach((zone, index) => this.renderZone(zone, index));
        }
        
        // Atualizar o minimapa sempre que as zonas forem renderizadas
        this.updateTrilhoMinimap();
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
        
        // Adicionar botão de gerenciamento de armazenamento
        this.addStorageManagementButton();
    }

    addStorageManagementButton() {
        // Procurar por um local adequado para adicionar o botão
        const header = document.querySelector('.header-actions');
        if (header) {
            const storageBtn = document.createElement('button');
            storageBtn.className = 'btn btn-sm btn-outline-secondary';
            storageBtn.innerHTML = '🗄️ Armazenamento';
            storageBtn.title = 'Gerenciar armazenamento local';
            storageBtn.addEventListener('click', () => {
                this.showStorageManagementDialog();
            });
            header.appendChild(storageBtn);
            console.log('✅ Botão de gerenciamento de armazenamento adicionado');
        }
    }

    showStorageManagementDialog() {
        const usage = this.checkLocalStorageUsage();
        
        const modal = document.createElement('div');
        modal.className = 'modal-overlay';
        modal.style.zIndex = '10000';
        modal.innerHTML = `
            <div class="modal-content" style="max-width: 600px;">
                <div class="modal-header">
                    <h3>🗄️ Gerenciamento de Armazenamento</h3>
                    <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="storage-summary">
                        <h4>Uso Atual: ${usage.totalSizeMB}MB</h4>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: ${Math.min(100, (usage.totalSize / (5 * 1024 * 1024)) * 100)}%"></div>
                        </div>
                        <small>Limite aproximado: 5MB</small>
                    </div>
                    <div class="storage-actions">
                        <h4>Ações Disponíveis:</h4>
                        <div class="action-buttons">
                            <button class="btn btn-info" onclick="window.trilhoConfigurator.showStorageDetails()">
                                📊 Ver Detalhes Completos
                            </button>
                            <button class="btn btn-warning" onclick="window.trilhoConfigurator.clearOldData()">
                                🧹 Limpar Dados Antigos
                            </button>
                            <button class="btn btn-danger" onclick="window.trilhoConfigurator.clearAllData()">
                                🗑️ Limpar Todos os Dados
                            </button>
                        </div>
                    </div>
                    <div class="storage-tips">
                        <h4>💡 Dicas:</h4>
                        <ul>
                            <li><strong>Limpar Dados Antigos:</strong> Remove versões antigas mantendo dados atuais</li>
                            <li><strong>Ver Detalhes:</strong> Mostra quais itens ocupam mais espaço</li>
                            <li><strong>Limpar Tudo:</strong> Remove todos os dados (use com cuidado)</li>
                        </ul>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-primary" onclick="this.closest('.modal-overlay').remove()">
                        Fechar
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Adicionar estilos se não existirem
        if (!document.getElementById('storage-management-styles')) {
            const style = document.createElement('style');
            style.id = 'storage-management-styles';
            style.textContent = `
                .storage-summary {
                    margin: 1rem 0;
                    padding: 1rem;
                    background: #f8f9fa;
                    border-radius: 8px;
                }
                .progress-bar {
                    width: 100%;
                    height: 20px;
                    background: #e0e0e0;
                    border-radius: 10px;
                    overflow: hidden;
                    margin: 0.5rem 0;
                }
                .progress-fill {
                    height: 100%;
                    background: linear-gradient(90deg, #4caf50, #ff9800, #f44336);
                    transition: width 0.3s ease;
                }
                .storage-actions {
                    margin: 1rem 0;
                }
                .action-buttons {
                    display: flex;
                    flex-direction: column;
                    gap: 0.5rem;
                    margin: 1rem 0;
                }
                .storage-tips {
                    background: #e3f2fd;
                    border: 1px solid #bbdefb;
                    border-radius: 8px;
                    padding: 1rem;
                    margin-top: 1rem;
                }
                .storage-tips ul {
                    margin: 0.5rem 0;
                    padding-left: 1.5rem;
                }
                .storage-tips li {
                    margin: 0.25rem 0;
                }
            `;
            document.head.appendChild(style);
        }
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
        console.log('✅ Estilos aplicados via CSS');
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
        
        // Para background, atualizar simulação visual
        if (section === 'background') {
            console.log('Atualizando simulação visual após reset do background...');
            setTimeout(() => {
                this.updateSimulationVisual();
                this.checkBackgroundDimensionsReady();
                console.log('Simulação visual atualizada após reset');
            }, 100);
        }
        
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

    // ===== NOVAS FUNCIONALIDADES =====

    initializeTVCalculations() {
        console.log('Inicializando cálculos de TV...');
        
        // TV model change
        const tvModelSelect = document.getElementById('tv-model');
        const tvOrientationSelect = document.getElementById('tv-orientation');
        
        console.log('Elementos da TV encontrados:', {
            tvModelSelect: !!tvModelSelect,
            tvOrientationSelect: !!tvOrientationSelect
        });
        
        if (tvModelSelect) {
            tvModelSelect.addEventListener('change', (e) => {
                console.log('Modelo da TV alterado');
                this.config.tv.model = e.target.value;
                this.calculateTVDimensions();
                this.updateSimulationVisual();
            });
        }
        
        if (tvOrientationSelect) {
            tvOrientationSelect.addEventListener('change', (e) => {
                console.log('Orientação da TV alterada');
                this.config.tv.orientation = e.target.value;
                this.calculateTVDimensions();
                this.updateSimulationVisual();
            });
        }
        
        // Calculate initial dimensions
        console.log('Calculando dimensões iniciais...');
        this.calculateTVDimensions();
        
        // Atualizar simulação visual inicial
        setTimeout(() => {
            this.updateSimulationVisual();
            this.checkBackgroundDimensionsReady();
        }, 500);
        
        // Forçar atualização adicional após carregamento completo
        setTimeout(() => {
            this.updateSimulationVisual();
            this.checkBackgroundDimensionsReady();
            console.log('Simulação visual forçada após carregamento completo');
        }, 1000);
        
        // Forçar atualização final após tudo estar carregado
        setTimeout(() => {
            this.updateSimulationVisual();
            console.log('Simulação visual final após carregamento completo');
        }, 2000);
    }

    calculateTVDimensions() {
        const tvModelElement = document.getElementById('tv-model');
        const orientationElement = document.getElementById('tv-orientation');
        
        console.log('Elementos encontrados:', {
            tvModelElement: !!tvModelElement,
            orientationElement: !!orientationElement
        });
        
        const tvModel = tvModelElement?.value || '42';
        const orientation = orientationElement?.value || 'portrait';
        
        console.log(`Calculando dimensões da TV: Modelo ${tvModel}" ${orientation}`);
        
        // Medidas reais das TVs
        const tvSpecs = {
            '42': { width: 93.5, height: 52.5 }, // TV 42" - medidas reais da tela
            '55': { width: 121, height: 68.5 }   // TV 55" - medidas reais da tela
        };
        
        let widthCm, heightCm;
        
        if (tvSpecs[tvModel]) {
            if (orientation === 'portrait') {
                // Vertical: altura é a largura da TV, largura é a altura da TV
                widthCm = tvSpecs[tvModel].height;   // 52.5cm ou 68.5cm
                heightCm = tvSpecs[tvModel].width;   // 93.5cm ou 121cm
            } else {
                // Horizontal: largura é a largura da TV, altura é a altura da TV
                widthCm = tvSpecs[tvModel].width;    // 93.5cm ou 121cm
                heightCm = tvSpecs[tvModel].height;  // 52.5cm ou 68.5cm
            }
            
            console.log(`Dimensões calculadas: ${widthCm}cm x ${heightCm}cm (${orientation})`);
            
            // Atualizar campos
            const widthInput = document.getElementById('screen-width-cm');
            const heightInput = document.getElementById('screen-height-cm');
            
            console.log('Campos encontrados:', {
                widthInput: !!widthInput,
                heightInput: !!heightInput
            });
            
            if (widthInput) {
                widthInput.value = widthCm.toFixed(1);
                console.log(`Largura definida: ${widthCm.toFixed(1)}cm`);
            }
            if (heightInput) {
                heightInput.value = heightCm.toFixed(1);
                console.log(`Altura definida: ${heightCm.toFixed(1)}cm`);
            }
            
            // Atualizar configuração
            this.updateConfigValue('trilho.screenWidthCm', widthCm);
            this.updateConfigValue('trilho.screenHeightCm', heightCm);
        } else {
            console.error(`Modelo de TV não encontrado: ${tvModel}`);
        }
    }

    initializeValidation() {
        console.log('Inicializando sistema de validação...');
        
        const validateBtn = document.getElementById('validate-config-btn');
        if (validateBtn) {
            validateBtn.addEventListener('click', () => {
                this.isManualValidation = true;
                this.validateConfiguration();
                this.isManualValidation = false;
            });
        }
        
        // Auto-validate on form changes
        document.addEventListener('input', () => this.autoValidate());
    }

    validateConfiguration() {
        console.log('Validando configuração...');
        
        const validation = {
            project: this.validateProjectInfo(),
            trilho: this.validateTrilhoConfig(),
            background: this.validateBackgroundConfig(),
            zones: this.validateZonesConfig(),
            osc: this.validateOSCConfig()
        };
        
        this.updateValidationUI(validation);
        
        const allValid = Object.values(validation).every(v => v.valid);
        
        // Só mostrar toast se for validação manual (não automática)
        if (this.isManualValidation) {
            if (allValid) {
                this.showToast('Configuração válida! Pronto para exportar.', 'success');
            } else {
                this.showToast('Configuração possui erros. Verifique os campos destacados.', 'warning');
            }
        }
        
        return allValid;
    }

    validateProjectInfo() {
        const name = document.getElementById('project-name')?.value?.trim();
        const location = document.getElementById('event-location')?.value?.trim();
        const client = document.getElementById('client-name')?.value?.trim();
        
        return {
            valid: !!(name && location && client),
            errors: [
                !name && 'Nome do projeto é obrigatório',
                !location && 'Local do evento é obrigatório',
                !client && 'Cliente é obrigatório'
            ].filter(Boolean)
        };
    }

    validateTrilhoConfig() {
        const maxCm = parseFloat(document.getElementById('physical-max-cm')?.value || 0);
        const screenWidth = parseFloat(document.getElementById('screen-width-cm')?.value || 0);
        
        return {
            valid: maxCm > 0 && screenWidth > 0,
            errors: [
                maxCm <= 0 && 'Comprimento do trilho deve ser maior que 0',
                screenWidth <= 0 && 'Largura da TV deve ser maior que 0'
            ].filter(Boolean)
        };
    }

    validateBackgroundConfig() {
        const enabled = document.getElementById('background-enabled')?.checked;
        const file = document.getElementById('background-image-upload')?.files[0];
        
        return {
            valid: !enabled || !!file,
            errors: [
                enabled && !file && 'Arquivo de background é obrigatório quando ativado'
            ].filter(Boolean)
        };
    }

    validateZonesConfig() {
        const hasZones = this.zones.length > 0;
        const validZones = this.zones.every(zone => 
            zone.name && zone.positionCm >= 0 && zone.positionCm <= this.config.trilho.physicalMaxCm
        );
        
        return {
            valid: hasZones && validZones,
            errors: [
                !hasZones && 'Pelo menos uma zona deve ser criada',
                !validZones && 'Todas as zonas devem ter nome e posição válida'
            ].filter(Boolean)
        };
    }

    validateOSCConfig() {
        const port = parseInt(document.getElementById('osc-port')?.value || 0);
        const address = document.getElementById('osc-address')?.value?.trim();
        
        return {
            valid: port > 1024 && port < 65536 && !!address,
            errors: [
                (port <= 1024 || port >= 65536) && 'Porta OSC deve estar entre 1025 e 65535',
                !address && 'Endereço OSC é obrigatório'
            ].filter(Boolean)
        };
    }

    updateValidationUI(validation) {
        const statusContainer = document.getElementById('validation-status');
        if (!statusContainer) return;
        
        const items = statusContainer.querySelectorAll('.validation-item');
        const sections = ['project', 'trilho', 'background', 'zones', 'osc'];
        
        sections.forEach((section, index) => {
            const item = items[index];
            if (!item) return;
            
            const icon = item.querySelector('i');
            const result = validation[section];
            
            if (result.valid) {
                icon.className = 'fas fa-check-circle validation-success';
                item.classList.remove('validation-error');
                item.classList.add('validation-success');
            } else {
                icon.className = 'fas fa-times-circle validation-error';
                item.classList.remove('validation-success');
                item.classList.add('validation-error');
            }
        });
        
        // Update summary
        const summary = document.getElementById('validation-summary');
        if (summary) {
            const allValid = Object.values(validation).every(v => v.valid);
            const errorCount = Object.values(validation).reduce((count, v) => count + v.errors.length, 0);
            
            if (allValid) {
                summary.innerHTML = '<p style="color: #4CAF50;">✅ Configuração válida! Pronto para exportar.</p>';
            } else {
                summary.innerHTML = `<p style="color: #f44336;">❌ ${errorCount} erro(s) encontrado(s). Verifique os campos destacados.</p>`;
            }
        }
    }

    autoValidate() {
        // Debounce validation to avoid excessive calls
        clearTimeout(this.validationTimeout);
        this.validationTimeout = setTimeout(() => {
            // Só validar se o usuário já interagiu com o formulário
            if (this.hasUserInteracted) {
                this.validateConfiguration();
            }
        }, 500);
    }

    initializeExport() {
        console.log('Inicializando funcionalidades de exportação...');
        
        // Export buttons
        const exportUnityBtn = document.getElementById('export-unity-btn');
        const exportJsonBtn = document.getElementById('export-json-btn');
        const exportBackupBtn = document.getElementById('export-backup-btn');
        
        if (exportUnityBtn) {
            exportUnityBtn.addEventListener('click', () => this.exportToUnity());
        }
        
        if (exportJsonBtn) {
            exportJsonBtn.addEventListener('click', () => this.exportJSON());
        }
        
        if (exportBackupBtn) {
            exportBackupBtn.addEventListener('click', () => this.exportBackup());
        }
        
        // Update package info
        this.updatePackageInfo();
    }

    async exportToUnity() {
        console.log('Exportando para Unity...');
        
        // Validate before export
        if (!this.validateConfiguration()) {
            this.showToast('Corrija os erros antes de exportar', 'error');
            return;
        }
        
        try {
            const configData = this.buildExportConfig();
            const mediaFiles = this.collectMediaFiles();
            
            if (mediaFiles.length > 0) {
                await this.createUnityPackage(configData, mediaFiles);
                this.showToast('Pacote Unity criado com sucesso!', 'success');
            } else {
                this.downloadJSON(configData, 'trilho_config.json');
                this.showToast('Configuração JSON salva!', 'success');
            }
            
            this.updateLastExport();
            
        } catch (error) {
            console.error('Erro ao exportar para Unity:', error);
            this.showToast('Erro ao exportar: ' + error.message, 'error');
        }
    }

    exportJSON() {
        console.log('Exportando JSON...');
        
        try {
            const configData = this.buildExportConfig();
            const filename = `trilho_config_${new Date().toISOString().split('T')[0]}.json`;
            this.downloadJSON(configData, filename);
            this.showToast('JSON exportado com sucesso!', 'success');
            this.updateLastExport();
        } catch (error) {
            console.error('Erro ao exportar JSON:', error);
            this.showToast('Erro ao exportar JSON: ' + error.message, 'error');
        }
    }

    exportBackup() {
        console.log('Criando backup...');
        
        try {
            const configData = this.buildExportConfig();
            const mediaFiles = this.collectMediaFiles();
            
            const backupData = {
                config: configData,
                mediaFiles: mediaFiles.map(f => ({
                    name: f.name,
                    type: f.type,
                    size: f.file.size
                })),
                exportDate: new Date().toISOString(),
                version: '1.0'
            };
            
            const filename = `trilho_backup_${new Date().toISOString().split('T')[0]}.json`;
            this.downloadJSON(backupData, filename);
            this.showToast('Backup criado com sucesso!', 'success');
            this.updateLastExport();
        } catch (error) {
            console.error('Erro ao criar backup:', error);
            this.showToast('Erro ao criar backup: ' + error.message, 'error');
        }
    }

    buildExportConfig() {
        return {
            configName: this.config.project.name,
            description: this.config.project.description,
            version: this.config.project.version,
            lastModified: this.config.lastModified,
            creationDate: this.config.creationDate,
            project: this.config.project,
            trilho: this.config.trilho,
            tv: this.config.tv,
            osc: this.config.osc,
            background: this.config.background,
            contentZones: this.zones.map(zone => ({
                id: zone.id,
                name: zone.name,
                description: zone.description || '',
                positionCm: zone.positionCm,
                paddingCm: zone.paddingCm || 20,
                enterPaddingCm: zone.enterPaddingCm || 10,
                exitPaddingCm: zone.exitPaddingCm || 10,
                contentType: zone.type,
                contentFileName: zone.imageSettings?.imageFile || zone.videoSettings?.videoFile || '',
                contentPath: zone.imageSettings?.imageFile || zone.videoSettings?.videoFile || '',
                placeContentAtWorldX: true,
                contentOffsetCm: 0,
                keepUpdatingWhileActive: false,
                reference: 0,
                fadeInSeconds: zone.fadeInSeconds || 1.0,
                fadeOutSeconds: zone.fadeOutSeconds || 0.5,
                imageSettings: zone.imageSettings || {},
                videoSettings: zone.videoSettings || {},
                textSettings: zone.textSettings || {},
                appSettings: zone.appSettings || {}
            }))
        };
    }

    updatePackageInfo() {
        const packageName = document.getElementById('package-name');
        const packageSize = document.getElementById('package-size');
        const packageFiles = document.getElementById('package-files');
        
        if (packageName) {
            const date = new Date().toISOString().split('T')[0];
            packageName.textContent = `trilho-config-${date}`;
        }
        
        if (packageFiles) {
            const fileCount = this.zones.length + (this.config.background.uploadedFile ? 1 : 0);
            packageFiles.textContent = `${fileCount} arquivo(s)`;
        }
        
        if (packageSize) {
            // Estimate size (rough calculation)
            const estimatedSize = this.estimatePackageSize();
            packageSize.textContent = estimatedSize;
        }
    }

    estimatePackageSize() {
        let totalSize = 0;
        
        // JSON size (rough estimate)
        totalSize += JSON.stringify(this.buildExportConfig()).length;
        
        // Media files size
        this.zones.forEach(zone => {
            if (zone.imageSettings?.uploadedFile) {
                totalSize += zone.imageSettings.uploadedFile.size;
            }
            if (zone.videoSettings?.uploadedFile) {
                totalSize += zone.videoSettings.uploadedFile.size;
            }
        });
        
        if (this.config.background.uploadedFile) {
            totalSize += this.config.background.uploadedFile.size;
        }
        
        // Convert to human readable
        if (totalSize < 1024) return `${totalSize} B`;
        if (totalSize < 1024 * 1024) return `${(totalSize / 1024).toFixed(1)} KB`;
        return `${(totalSize / (1024 * 1024)).toFixed(1)} MB`;
    }

    updateLastExport() {
        const lastExport = document.getElementById('last-export');
        if (lastExport) {
            lastExport.textContent = new Date().toLocaleString('pt-BR');
        }
    }

    showSystemInfo() {
        const systemSection = document.getElementById('system-info-section');
        if (systemSection) {
            systemSection.style.display = 'block';
            
            // Atualizar datas
            const creationDate = document.getElementById('creation-date');
            const lastModified = document.getElementById('last-modified');
            
            if (creationDate) {
                creationDate.value = this.config.creationDate ? 
                    new Date(this.config.creationDate).toLocaleString('pt-BR') : 
                    'Não definida';
            }
            
            if (lastModified) {
                lastModified.value = this.config.lastModified ? 
                    new Date(this.config.lastModified).toLocaleString('pt-BR') : 
                    'Não definida';
            }
        }
    }
}


// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.trilhoConfigurator = new TrilhoConfigurator();
});

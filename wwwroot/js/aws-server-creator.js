function toggleConfigType() {
    const standardConfig = document.getElementById('standardConfig').checked;
    const standardSection = document.getElementById('standardConfigSection');
    const customSection = document.getElementById('customConfigSection');
    
    if (standardConfig) {
        standardSection.style.display = 'block';
        customSection.style.display = 'none';
    } else {
        standardSection.style.display = 'none';
        customSection.style.display = 'block';
    }
}

function selectConfig(index) {
    const configCards = document.querySelectorAll('.config-card');
    configCards.forEach(card => card.classList.remove('selected'));
    
    const selectedCard = document.querySelector(`#config${index}`).closest('.config-card');
    if (selectedCard) {
        selectedCard.classList.add('selected');
    }
    
    document.getElementById('config' + index).checked = true;
}

function toggleKeyPairOption() {
    const createNewKey = document.getElementById('createNewKey').checked;
    const useExistingKey = document.getElementById('useExistingKey').checked;
    const uploadPublicKey = document.getElementById('uploadPublicKey').checked;
    
    const existingKeySection = document.getElementById('existingKeySection');
    const uploadKeySection = document.getElementById('uploadKeySection');
    

    existingKeySection.style.display = 'none';
    uploadKeySection.style.display = 'none';

    if (useExistingKey) {
        existingKeySection.style.display = 'block';
    } else if (uploadPublicKey) {
        uploadKeySection.style.display = 'block';
    }
}

async function controlInstance(instanceId, action) {
    if (action === 'terminate' && !confirm('Are you sure you want to terminate this instance? This action cannot be undone.')) {
        return;
    }
    
    if (action === 'stop' && !confirm('Are you sure you want to stop this instance?')) {
        return;
    }
    
    const button = event.target.closest('button');
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
    button.disabled = true;
    
    const accessKey = document.getElementById('AccessKey').value;
    const secretKey = document.getElementById('SecretKey').value;
    const region = document.getElementById('Region').value;
    
    if (!accessKey || !secretKey) {
        alert('Please enter AWS credentials first');
        button.innerHTML = originalText;
        button.disabled = false;
        return;
    }
    
    try {
        const response = await fetch(`/api/servers/${action}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                instanceId: instanceId,
                accessKey: accessKey,
                secretKey: secretKey,
                region: region
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(`${action.charAt(0).toUpperCase() + action.slice(1)} operation initiated successfully for instance ${instanceId}`);
            setTimeout(() => {
                document.querySelector('form').submit();
            }, 2000);
        } else {
            alert(`Error: ${result.message}`);
        }
    } catch (error) {
        alert(`Error performing ${action}: ${error.message}`);
    } finally {
        button.innerHTML = originalText;
        button.disabled = false;
    }
}

function validateAwsCredentials() {
    const accessKey = document.getElementById('AccessKey').value.trim();
    const secretKey = document.getElementById('SecretKey').value.trim();
    
    if (!accessKey || !secretKey) {
        alert('Please enter both Access Key ID and Secret Access Key');
        return false;
    }
    
    if (accessKey.length < 16 || accessKey.length > 32) {
        alert('Access Key ID format appears to be invalid');
        return false;
    }
    
    if (secretKey.length < 40) {
        alert('Secret Access Key format appears to be invalid');
        return false;
    }
    
    return true;
}

function validateKeyFile() {
    const fileInput = document.getElementById('KeyFile');
    if (fileInput && fileInput.files.length > 0) {
        const file = fileInput.files[0];
        const fileName = file.name.toLowerCase();
        
        if (!fileName.endsWith('.pem') && !fileName.endsWith('.pub')) {
            alert('Please upload a valid key file (.pem or .pub)');
            return false;
        }
        
        if (file.size > 10240) {
            alert('Key file is too large. Maximum size is 10KB.');
            return false;
        }
    }
    
    return true;
}

function initializePage() {
    const configCards = document.querySelectorAll('.config-card');
    configCards.forEach((card, index) => {
        card.addEventListener('click', () => selectConfig(index));
    });
    
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', (e) => {
            if (!validateAwsCredentials()) {
                e.preventDefault();
                return false;
            }
            
            if (!validateKeyFile()) {
                e.preventDefault();
                return false;
            }
        });
    });
    
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            const form = button.closest('form');
            if (form && form.checkValidity()) {
                const originalText = button.innerHTML;
                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
                button.disabled = true;
                
                setTimeout(() => {
                    button.innerHTML = originalText;
                    button.disabled = false;
                }, 30000);
            }
        });
    });
}

function autoRefreshServers() {
    const refreshInterval = 30000;
    const currentPage = window.location.pathname;
    
    if (currentPage.includes('Servers')) {
        setInterval(() => {
            const form = document.querySelector('form[method="post"]');
            if (form) {
                const accessKey = document.getElementById('AccessKey');
                const secretKey = document.getElementById('SecretKey');
                
                if (accessKey && secretKey && accessKey.value && secretKey.value) {
                    const hiddenSubmit = document.createElement('input');
                    hiddenSubmit.type = 'submit';
                    hiddenSubmit.style.display = 'none';
                    form.appendChild(hiddenSubmit);
                    hiddenSubmit.click();
                    form.removeChild(hiddenSubmit);
                }
            }
        }, refreshInterval);
    }
}

function showNotification(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        <i class="fas fa-info-circle"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
        
        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString();
}

document.addEventListener('DOMContentLoaded', function() {
    initializePage();
});

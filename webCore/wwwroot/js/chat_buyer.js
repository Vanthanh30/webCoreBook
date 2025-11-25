// Dữ liệu mẫu cho danh sách khách hàng
const customerChats = [
    {
        id: 1,
        name: "Nguyễn Văn An",
        avatar: "/images/customer1.png",
        lastMessage: "Em muốn hỏi về cuốn Sách Lập Trình C# có còn không ạ?",
        time: "10:52",
        unread: true,
        email: "nguyenvanan@gmail.com",
        phone: "0987654321"
    },
    {
        id: 2,
        name: "Trần Thị Bình",
        avatar: "/images/customer2.png",
        lastMessage: "Cảm ơn shop, em đã nhận được sách rồi ạ!",
        time: "09:30",
        unread: false,
        email: "tranthibinh@gmail.com",
        phone: "0912345678"
    },
    {
        id: 3,
        name: "Lê Hoàng Cường",
        avatar: "/images/customer3.png",
        lastMessage: "Đơn hàng của em khi nào giao được ạ?",
        time: "08:15",
        unread: true,
        email: "lehoangcuong@gmail.com",
        phone: "0901234567"
    },
    {
        id: 4,
        name: "Phạm Thị Dung",
        avatar: "/images/customer4.png",
        lastMessage: "Shop có sách về Marketing không ạ?",
        time: "Hôm qua",
        unread: false,
        email: "phamthidung@gmail.com",
        phone: "0898765432"
    },
    {
        id: 5,
        name: "Hoàng Văn Em",
        avatar: "/images/customer5.png",
        lastMessage: "Em muốn đổi sách được không shop?",
        time: "Hôm qua",
        unread: true,
        email: "hoangvanem@gmail.com",
        phone: "0976543210"
    }
];

// Dữ liệu mẫu cho tin nhắn
const customerMessages = {
    1: [
        {
            id: 1,
            type: "received",
            text: "Chào shop, em muốn hỏi về mấy cuốn sách lập trình ạ",
            time: "10:45",
            avatar: "/images/customer1.png"
        },
        {
            id: 2,
            type: "sent",
            text: "Chào bạn Nguyễn Văn An! Shop có nhiều sách lập trình lắm bạn ơi. Bạn đang tìm sách về ngôn ngữ nào vậy?",
            time: "10:46",
            avatar: "/images/seller-avatar.png"
        },
        {
            id: 3,
            type: "received",
            text: "Em muốn học C# ạ, shop có sách nào hay giới thiệu không?",
            time: "10:50",
            avatar: "/images/customer1.png"
        },
        {
            id: 4,
            type: "sent",
            text: "Shop có mấy cuốn hay về C# này, để shop gửi bạn xem nhé!",
            time: "10:51",
            avatar: "/images/seller-avatar.png"
        }
    ],
    2: [
        {
            id: 1,
            type: "received",
            text: "Cảm ơn shop, em đã nhận được sách rồi ạ! Sách đóng gói rất đẹp và cẩn thận",
            time: "09:30",
            avatar: "/images/customer2.png"
        },
        {
            id: 2,
            type: "sent",
            text: "Cảm ơn bạn đã tin tưởng shop! Chúc bạn đọc sách vui vẻ nhé. Nếu có gì thắc mắc cứ nhắn cho shop nha! 📚",
            time: "09:32",
            avatar: "/images/seller-avatar.png"
        }
    ]
};

// Danh sách emoji phổ biến
const emojiList = [
    '😀', '😃', '😄', '😁', '😆', '😅', '🤣', '😂', '🙂', '🙃',
    '😉', '😊', '😇', '🥰', '😍', '🤩', '😘', '😗', '😚', '😙',
    '😋', '😛', '😜', '🤪', '😝', '🤑', '🤗', '🤭', '🤫', '🤔',
    '🤐', '🤨', '😐', '😑', '😶', '😏', '😒', '🙄', '😬', '🤥',
    '😌', '😔', '😪', '🤤', '😴', '😷', '🤒', '🤕', '🤢', '🤮',
    '🤧', '🥵', '🥶', '😶‍🌫️', '🥴', '😵', '🤯', '🤠', '🥳', '😎',
    '🤓', '🧐', '😕', '😟', '🙁', '😮', '😯', '😲', '😳', '🥺',
    '😦', '😧', '😨', '😰', '😥', '😢', '😭', '😱', '😖', '😣',
    '😞', '😓', '😩', '😫', '🥱', '😤', '😡', '😠', '🤬', '😈',
    '👋', '🤚', '🖐', '✋', '🖖', '👌', '🤏', '✌️', '🤞', '🤟',
    '🤘', '🤙', '👈', '👉', '👆', '🖕', '👇', '☝️', '👍', '👎',
    '✊', '👊', '🤛', '🤜', '👏', '🙌', '👐', '🤲', '🤝', '🙏',
    '❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '🤍', '🤎', '💔',
    '❣️', '💕', '💞', '💓', '💗', '💖', '💘', '💝', '💟', '☮️',
    '✝️', '☪️', '🕉', '☸️', '✡️', '🔯', '🕎', '☯️', '☦️', '🛐',
    '⭐', '🌟', '✨', '⚡', '☄️', '💥', '🔥', '🌈', '☀️', '🌤',
    '⛅', '🌥', '☁️', '🌦', '🌧', '⛈', '🌩', '🌨', '❄️', '☃️',
    '📚', '📖', '📕', '📗', '📘', '📙', '📔', '📓', '📒', '📃'
];

// Biến lưu trạng thái
let currentCustomerId = null;
let currentMessages = [];
let emojiPickerVisible = false;
let pendingFiles = []; // Lưu file đang chờ gửi

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    loadCustomerList();
    setupEventListeners();
    createEmojiPicker();
});

// Load danh sách khách hàng
function loadCustomerList() {
    const chatList = document.getElementById('chatList');
    chatList.innerHTML = '';

    customerChats.forEach(customer => {
        const chatItem = createChatItem(customer);
        chatList.appendChild(chatItem);
    });
}

// Tạo phần tử chat item
function createChatItem(customer) {
    const div = document.createElement('div');
    div.className = `chat-item ${customer.id === currentCustomerId ? 'active' : ''}`;
    div.onclick = () => selectCustomer(customer.id);

    div.innerHTML = `
        <img src="${customer.avatar}" alt="${customer.name}" class="chat-item-avatar" 
             onerror="this.src='https://ui-avatars.com/api/?name=${encodeURIComponent(customer.name)}&background=random'">
        <div class="chat-item-content">
            <div class="chat-item-header">
                <span class="chat-item-name">${customer.name}</span>
                <span class="chat-item-time">${customer.time}</span>
            </div>
            <div class="chat-item-message">${customer.lastMessage}</div>
        </div>
        ${customer.unread ? '<div class="unread-indicator"></div>' : ''}
    `;

    return div;
}

// Chọn khách hàng
function selectCustomer(customerId) {
    currentCustomerId = customerId;

    // Update active state
    document.querySelectorAll('.chat-item').forEach(item => {
        item.classList.remove('active');
    });
    event.currentTarget.classList.add('active');

    // Load messages
    loadMessages(customerId);

    // Update customer info in header
    const customer = customerChats.find(c => c.id === customerId);
    if (customer) {
        document.getElementById('headerName').textContent = customer.name;
        document.getElementById('headerAvatar').src = customer.avatar;
        document.getElementById('headerAvatar').onerror = function () {
            this.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(customer.name)}&background=random`;
        };
    }
}

// Load tin nhắn
function loadMessages(customerId) {
    const chatMessages = document.getElementById('chatMessages');
    chatMessages.innerHTML = '';

    const messages = customerMessages[customerId] || [];
    currentMessages = messages;

    if (messages.length === 0) {
        chatMessages.innerHTML = `
            <div class="text-center text-muted py-5">
                <i class="bi bi-chat-text fs-1"></i>
                <p class="mt-2">Chưa có tin nhắn nào. Hãy bắt đầu trò chuyện!</p>
            </div>
        `;
        return;
    }

    messages.forEach(message => {
        const messageElement = createMessageElement(message);
        chatMessages.appendChild(messageElement);
    });

    // Scroll to bottom
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Tạo phần tử tin nhắn
function createMessageElement(message) {
    const div = document.createElement('div');
    div.className = `message ${message.type}`;

    const avatarSrc = message.avatar || `https://ui-avatars.com/api/?name=User&background=random`;
    const avatarHTML = message.type === 'received'
        ? `<img src="${avatarSrc}" alt="Avatar" class="message-avatar" 
               onerror="this.src='https://ui-avatars.com/api/?name=User&background=random'">`
        : '';

    // Check if message has image or file
    let contentHTML = '';
    if (message.image) {
        contentHTML = `<img src="${message.image}" alt="Image" class="message-image" style="max-width: 300px; border-radius: 8px;">`;
    } else if (message.file) {
        contentHTML = `
            <div class="message-file">
                <i class="bi bi-file-earmark-text"></i>
                <span>${message.fileName}</span>
            </div>
        `;
    }

    if (message.text) {
        contentHTML += `<div class="message-text">${message.text}</div>`;
    }

    div.innerHTML = `
        ${avatarHTML}
        <div class="message-content">
            ${contentHTML}
            <div class="message-time">
                <span class="message-time-text">${message.time}</span>
                ${message.type === 'sent' ? '<i class="bi bi-check-all message-status"></i>' : ''}
            </div>
        </div>
    `;

    return div;
}

// Tạo emoji picker
function createEmojiPicker() {
    const emojiPicker = document.createElement('div');
    emojiPicker.id = 'emojiPicker';
    emojiPicker.className = 'emoji-picker';
    emojiPicker.style.display = 'none';

    const emojiGrid = document.createElement('div');
    emojiGrid.className = 'emoji-grid';

    emojiList.forEach(emoji => {
        const emojiBtn = document.createElement('button');
        emojiBtn.className = 'emoji-item';
        emojiBtn.textContent = emoji;
        emojiBtn.type = 'button';
        emojiBtn.onclick = (e) => {
            e.stopPropagation();
            insertEmoji(emoji);
        };
        emojiGrid.appendChild(emojiBtn);
    });

    emojiPicker.appendChild(emojiGrid);
    document.body.appendChild(emojiPicker);

    // Close emoji picker when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('#emojiPicker') && !e.target.closest('#btnEmoji')) {
            hideEmojiPicker();
        }
    });
}

// Toggle emoji picker
function toggleEmojiPicker() {
    const emojiPicker = document.getElementById('emojiPicker');
    const btnEmoji = document.getElementById('btnEmoji');

    if (emojiPickerVisible) {
        hideEmojiPicker();
    } else {
        const rect = btnEmoji.getBoundingClientRect();
        emojiPicker.style.bottom = `${window.innerHeight - rect.top + 5}px`;
        emojiPicker.style.left = `${rect.left}px`;
        emojiPicker.style.display = 'block';
        emojiPickerVisible = true;
    }
}

// Hide emoji picker
function hideEmojiPicker() {
    const emojiPicker = document.getElementById('emojiPicker');
    emojiPicker.style.display = 'none';
    emojiPickerVisible = false;
}

// Insert emoji vào input
function insertEmoji(emoji) {
    const messageInput = document.getElementById('messageInput');
    const cursorPos = messageInput.selectionStart;
    const textBefore = messageInput.value.substring(0, cursorPos);
    const textAfter = messageInput.value.substring(cursorPos);

    messageInput.value = textBefore + emoji + textAfter;
    messageInput.focus();

    // Set cursor position after emoji
    const newPos = cursorPos + emoji.length;
    messageInput.setSelectionRange(newPos, newPos);
}

// Handle file selection
function handleFileSelect(files) {
    if (!currentCustomerId) {
        alert('Vui lòng chọn khách hàng để gửi file!');
        return;
    }

    Array.from(files).forEach(file => {
        const fileData = {
            file: file,
            isImage: file.type.startsWith('image/'),
            preview: null
        };

        if (fileData.isImage) {
            // Read image for preview
            const reader = new FileReader();
            reader.onload = function (e) {
                fileData.preview = e.target.result;
                pendingFiles.push(fileData);
                updateFilePreview();
            };
            reader.readAsDataURL(file);
        } else {
            pendingFiles.push(fileData);
            updateFilePreview();
        }
    });
}

// Update file preview area
function updateFilePreview() {
    const previewArea = document.getElementById('filePreviewArea');
    previewArea.innerHTML = '';

    if (pendingFiles.length === 0) {
        previewArea.classList.remove('active');
        return;
    }

    previewArea.classList.add('active');

    pendingFiles.forEach((fileData, index) => {
        const previewItem = document.createElement('div');
        previewItem.className = 'file-preview-item';

        let contentHTML = '';
        if (fileData.isImage && fileData.preview) {
            contentHTML = `
                <img src="${fileData.preview}" alt="Preview" class="file-preview-image">
                <div class="file-preview-info">
                    <div class="file-preview-name">${fileData.file.name}</div>
                    <div class="file-preview-size">${formatFileSize(fileData.file.size)}</div>
                </div>
            `;
        } else {
            contentHTML = `
                <i class="bi bi-file-earmark-text file-preview-icon"></i>
                <div class="file-preview-info">
                    <div class="file-preview-name">${fileData.file.name}</div>
                    <div class="file-preview-size">${formatFileSize(fileData.file.size)}</div>
                </div>
            `;
        }

        previewItem.innerHTML = `
            ${contentHTML}
            <div class="file-preview-remove" onclick="removePendingFile(${index})">
                <i class="bi bi-x"></i>
            </div>
        `;

        previewArea.appendChild(previewItem);
    });
}

// Format file size
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

// Remove pending file
function removePendingFile(index) {
    pendingFiles.splice(index, 1);
    updateFilePreview();
}

// Send all pending files
function sendPendingFiles() {
    if (pendingFiles.length === 0) return;

    pendingFiles.forEach(fileData => {
        if (fileData.isImage) {
            sendImageMessage(fileData.preview, fileData.file.name);
        } else {
            sendFileMessage(fileData.file);
        }
    });

    // Clear pending files
    pendingFiles = [];
    updateFilePreview();
}

// Send image message
function sendImageMessage(imageData, fileName) {
    const newMessage = {
        id: (currentMessages.length || 0) + 1,
        type: 'sent',
        image: imageData,
        text: '',
        time: getCurrentTime(),
        avatar: '/images/seller-avatar.png'
    };

    addAndDisplayMessage(newMessage, `📷 Hình ảnh`);
}

// Send file message
function sendFileMessage(file) {
    const newMessage = {
        id: (currentMessages.length || 0) + 1,
        type: 'sent',
        file: true,
        fileName: file.name,
        text: '',
        time: getCurrentTime(),
        avatar: '/images/seller-avatar.png'
    };

    addAndDisplayMessage(newMessage, `📎 ${file.name}`);
}

// Add and display message
function addAndDisplayMessage(newMessage, lastMessageText) {
    if (!customerMessages[currentCustomerId]) {
        customerMessages[currentCustomerId] = [];
    }
    customerMessages[currentCustomerId].push(newMessage);
    currentMessages.push(newMessage);

    const chatMessages = document.getElementById('chatMessages');
    const emptyState = chatMessages.querySelector('.text-center');
    if (emptyState) {
        emptyState.remove();
    }

    const messageElement = createMessageElement(newMessage);
    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;

    updateCustomerLastMessage(currentCustomerId, lastMessageText);
}

// Setup event listeners
function setupEventListeners() {
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');

    // Send message on button click
    sendBtn.addEventListener('click', sendMessage);

    // Send message on Enter key
    messageInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    // Emoji button
    const btnEmoji = document.getElementById('btnEmoji');
    if (btnEmoji) {
        btnEmoji.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleEmojiPicker();
        });
    }

    // Image button
    const btnImage = document.getElementById('btnImage');
    if (btnImage) {
        btnImage.addEventListener('click', function () {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = 'image/*';
            input.multiple = true;
            input.onchange = (e) => handleFileSelect(e.target.files);
            input.click();
        });
    }

    // File button
    const btnFile = document.getElementById('btnFile');
    if (btnFile) {
        btnFile.addEventListener('click', function () {
            const input = document.createElement('input');
            input.type = 'file';
            input.multiple = true;
            input.onchange = (e) => handleFileSelect(e.target.files);
            input.click();
        });
    }

    // Search functionality
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('input', function (e) {
            const searchTerm = e.target.value.toLowerCase();
            filterCustomerList(searchTerm);
        });
    }
}

// Gửi tin nhắn
function sendMessage() {
    if (!currentCustomerId) {
        alert('Vui lòng chọn khách hàng để chat!');
        return;
    }

    const messageInput = document.getElementById('messageInput');
    const messageText = messageInput.value.trim();

    // Check if there's text or files to send
    if (messageText === '' && pendingFiles.length === 0) return;

    // Send text message if exists
    if (messageText !== '') {
        const newMessage = {
            id: (currentMessages.length || 0) + 1,
            type: 'sent',
            text: messageText,
            time: getCurrentTime(),
            avatar: '/images/seller-avatar.png'
        };

        if (!customerMessages[currentCustomerId]) {
            customerMessages[currentCustomerId] = [];
        }
        customerMessages[currentCustomerId].push(newMessage);
        currentMessages.push(newMessage);

        const chatMessages = document.getElementById('chatMessages');
        const emptyState = chatMessages.querySelector('.text-center');
        if (emptyState) {
            emptyState.remove();
        }

        const messageElement = createMessageElement(newMessage);
        chatMessages.appendChild(messageElement);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        updateCustomerLastMessage(currentCustomerId, messageText);
    }

    // Send pending files
    sendPendingFiles();

    // Clear input
    messageInput.value = '';
}

// Cập nhật tin nhắn cuối trong danh sách
function updateCustomerLastMessage(customerId, message) {
    const customer = customerChats.find(c => c.id === customerId);
    if (customer) {
        customer.lastMessage = message;
        customer.time = getCurrentTime();
        customer.unread = false;
        loadCustomerList();

        // Re-select current customer to maintain active state
        const currentItem = document.querySelector(`.chat-item[onclick*="${customerId}"]`);
        if (currentItem) {
            currentItem.classList.add('active');
        }
    }
}

// Lấy thời gian hiện tại
function getCurrentTime() {
    const now = new Date();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes}`;
}

// Lọc danh sách khách hàng
function filterCustomerList(searchTerm) {
    const chatList = document.getElementById('chatList');
    chatList.innerHTML = '';

    const filteredCustomers = customerChats.filter(customer =>
        customer.name.toLowerCase().includes(searchTerm) ||
        customer.phone.includes(searchTerm) ||
        customer.lastMessage.toLowerCase().includes(searchTerm)
    );

    if (filteredCustomers.length === 0) {
        chatList.innerHTML = '<div class="text-center text-muted py-4">Không tìm thấy khách hàng</div>';
        return;
    }

    filteredCustomers.forEach(customer => {
        const chatItem = createChatItem(customer);
        chatList.appendChild(chatItem);
    });
}
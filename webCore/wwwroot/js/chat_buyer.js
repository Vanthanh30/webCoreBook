const buyerId = document.getElementById("buyerId").value;
let selectedSellerId = null;
let orderId = null;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub?userId=" + buyerId)
    .build();

connection.on("ReceiveMessage", (msg) => {
    if (msg.SenderId === selectedSellerId || msg.ReceiverId === selectedSellerId) {
        appendMessage(msg, msg.SenderId === buyerId);
    }
});

connection.start().catch(err => console.error(err.toString()));

document.getElementById("sendBtn").addEventListener("click", async () => {
    const input = document.getElementById("messageInput");
    if (!input.value || !selectedSellerId) return;

    const msg = {
        SenderId: buyerId,
        ReceiverId: selectedSellerId,
        OrderId: orderId,
        MessageText: input.value,
        Type: "Text",
        CreatedAt: new Date()
    };

    await fetch("/api/chat/send-text", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(msg)
    });

    input.value = "";
});

function appendMessage(msg, isMine) {
    const chatMessages = document.getElementById("chatMessages");
    const div = document.createElement("div");
    div.className = isMine ? "message message-sent" : "message message-received";

    if (msg.Type === "Text") div.innerText = msg.MessageText;
    if (msg.Type === "Image") div.innerHTML = `<img src="${msg.ImageUrl}" class="chat-img"/>`;
    chatMessages.appendChild(div);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

document.querySelectorAll(".chat-list-item").forEach(el => {
    el.addEventListener("click", async () => {
        selectedSellerId = el.dataset.userid;
        orderId = el.dataset.orderid;

        const res = await fetch(`/api/chat/history/${orderId}`);
        const msgs = await res.json();

        const chatMessages = document.getElementById("chatMessages");
        chatMessages.innerHTML = "";
        msgs.forEach(msg => appendMessage(msg, msg.SenderId === buyerId));

        document.getElementById("headerName").innerText = el.dataset.name;
        document.getElementById("headerAvatar").src = el.dataset.avatar || "/images/avatar-default.png";
    });
});

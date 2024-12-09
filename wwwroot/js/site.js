// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//function createDoctorCard(data) {
//    const doctorCardContainer = document.getElementById('doctor-card-container');

//    const card = document.createElement('div');
//    card.className = 'ms-DocumentCard root-500';
//    card.setAttribute('role', 'group');
//    card.setAttribute('aria-label', 'Doctor Card');

//    card.innerHTML = `
//        <a href="@Url.Action("DoctorDetails", "Doctor", new { id = "<insert_id_here>" })" style="text-decoration: none; color: inherit;">
//            <div class="root-501">
//                <div class="ms-Image root-517" style="height: 150px;">
//                    <img src="${data.ImageSrc}" role="presentation" alt="" class="ms-Image-image is-loaded ms-Image-image--centerCover ms-Image-image--portrait is-fadeIn image-518">
//                </div>
//                <i aria-hidden="true" class="cornerIcon-519"></i>
//            </div>
//            <div class="ms-DocumentCardDetails root-508">
//                <div class="sc-fTABeZ sc-hRxedE hsqAaa dntOkG">
//                    <span title="${data.Name}" class="css-509">${data.Name}</span>
//                </div>
//                <div class="sc-fTABeZ hsqAaa">
//                    <span class="css-510">Degree: ${data.Degree}</span>
//                </div>
//                <div class="sc-fTABeZ hsqAaa">
//                    <span class="css-511">Available: ${data.AvailabilityHours}</span>
//                </div>
//                <div class="sc-fTABeZ hsqAaa">
//                    <span class="css-511">Hospital: ${data.Hospital}</span>
//                </div>
//            </div>
//        </a>
//    `;

//    doctorCardContainer.appendChild(card);
//}
//const connection = new signalR.HubConnectionBuilder()
//    .withUrl("/chatHub")
//    .build();

//connection.start()
//    .catch(err => console.error("Error establishing connection: ", err));

//// Handle chat list item click
//$(".chat-list-item").on("click", function () {
//    const chatId = $(this).data("chat-id");
//    const participantId = $(this).data("participant-id");

//    // You can now load the selected chat's messages using AJAX
//    loadChatMessages(chatId, participantId);
//});

//function loadChatMessages(chatId, participantId) {
//    $.get(`/Chats/GetMessages?chatId=${chatId}`, function (data) {
//        $("#chat-messages").empty(); // Clear previous messages
//        data.messages.forEach(function (message) {
//            const messageClass = message.senderRole === "Patient" ? "receive" : "send";
//            const messageHtml = `
//                <div class="message ${messageClass}">
//                    <p>${message.text}</p>
//                    <span class="time">${message.timeSent}</span>
//                </div>
//            `;
//            $("#chat-messages").append(messageHtml);
//        });
//        $("#recipient-id").val(participantId); // Set the recipient for sending new messages
//    });
//}


//// New Chat Button (Open Modal)
//$("#new-chat-btn").on("click", function () {
//    $("#new-chat-modal").fadeIn();

//    // Fetch new participants
//    $.get("/Chats/NewParticipants", function (data) {
//        const participantList = $("#participant-list").empty();
//        data.forEach(participant => {
//            participantList.append(`<li class="participant-item" data-id="${participant.participantID}">${participant.participantName}</li>`);
//        });
//    });
//});

//// Handle participant selection
//$("#participant-list").on("click", ".participant-item", function () {
//    const participantId = $(this).data("id");
//    const participantName = $(this).text();
//    console.log(participantId);

//    $("#new-chat-modal").fadeOut();
//    $("#recipient-id").val(participantId);
//    $("#chat-messages").html(`<div class="message">Started chat with ${participantName}</div>`);
//});

//// Close Modal
//$("#close-modal").on("click", function () {
//    $("#new-chat-modal").fadeOut();
//});

////// Function to set recipient ID when a participant is selected
////$(".chat-list-item").on("click", function () {
////    const participantId = $(this).data("id");
////    if (participantId) {
////        // Set hidden input or directly use the participant ID
////        $("#recipient-id").val(participantId);
////        console.log("Participant ID set to:", participantId);
////    } else {
////        console.error("Failed to retrieve participant ID from chat-list-item");
////    }
////});

//$("#chat-form").submit(function (e) {
//    e.preventDefault();

//    const recipientId = $("#recipient-id").val();  // Get the selected recipient ID
//    const message = $("#message-input").val();     // Get the message from the input field

//    // Check if message is not empty
//    if (message.trim() && recipientId) {
//        // Send the message via AJAX
//        $.post("/Chats/SendMessage", { recipientId, message }, function (data) {
//            // After successful message send, append the message to the chat
//            const newMessage = `
//                <div class="message send">
//                    <p>${message}</p>
//                    <span class="time">${data.timeSent}</span>
//                </div>
//            `;
//            $("#chat-messages").append(newMessage);
//            $("#message-input").val("");  // Clear the input field
//            $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);  // Auto-scroll to the bottom
//        });
//    }
//});

//// Handle receiving messages
//connection.on("ReceiveMessage", (recipientId, message, timeSent, senderRole) => {
//    if (recipientId == $("#recipient-id").val()) {
//        const messageClass = senderRole == "Patient" ? "receive" : "send";
//        const newMessage = `
//            <div class="message ${messageClass}">
//                <p>${message}</p>
//                <span class="time">${timeSent}</span>
//            </div>
//        `;
//        $("#chat-messages").append(newMessage);
//        $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);  // Auto-scroll to the bottom
//    }
//});







//// Real-time message handling
//connection.on("ReceiveMessage", (recipientId, message, timeSent, senderRole) => {
//    const currentRecipientId = parseInt($("#recipient-id").val());
//    if (recipientId === currentRecipientId) {
//        const cssClass = senderRole === "Patient" ? "patient" : "doctor";
//        $("#chat-messages").append(`<div class="message ${cssClass}">${message} <small>${new Date(timeSent).toLocaleTimeString()}</small></div>`);
//    }
//});

//connection.on("ReceiveMessage", (doctorId, patientId, message, timeSent) => {
//    const recipientId = parseInt($("#recipient-id").val());

//    // Append the message only if it's relevant to the current chat
//    if (recipientId === doctorId || recipientId === patientId) {
//        const timestamp = new Date(timeSent).toLocaleTimeString();
//        const cssClass = recipientId === doctorId ? "receiver" : "sender";

//        $("#chat-messages").append(`
//            <div class="message ${cssClass}">
//                <div>${message}</div>
//                <small>${timestamp}</small>
//            </div>
//        `);
//    }
//});


//// Handle receiving messages
//connection.on("ReceiveMessage", (recipientId, message, timeSent) => {
//    if (recipientId == $("#recipient-id").val()) {
//        const newMessage = `
//            <div class="message receive">
//                <p>${message}</p>
//                <span class="time">${new Date(timeSent).toLocaleTimeString()}</span>
//            </div>
//        `;
//        $("#chat-messages").append(newMessage);
//        $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);  // Auto-scroll to the bottom
//    }
//});





// Initialize SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start()
    .catch(err => console.error("Error establishing connection: ", err));

// Handle chat list item click to load messages
$(".chat-list-item").on("click", function () {
    const chatId = $(this).data("chat-id");
    const participantId = $(this).data("participant-id");

    // Load the selected chat's messages
    loadChatMessages(participantId);
});
//Load Chat Messages
function loadChatMessages(participantId) {
    $.get(`/Chats/History?participantId=${participantId}`, function (data) {
        $("#chat-messages").empty(); // Clear previous messages

        data.forEach(function (message) {
            const messageClass = message.senderRole === "Patient" ? "receive" : "send";
            const messageHtml = `
                <div class="message ${messageClass}">
                    <p>${message.message}</p>
                    <span class="time">${message.timeSent}</span>
                </div>
            `;
            $("#chat-messages").append(messageHtml);
        });

        // Scroll to the bottom
        $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);
    }).fail(function (response) {
        alert(response.responseJSON.message); // Display error message if no chat found
    });
}

// New Chat Button (Open Modal)
$("#new-chat-btn").on("click", function () {
    $("#new-chat-modal").fadeIn();

    // Fetch participants for new chat
    $.get("/Chats/NewParticipants", function (data) {
        const participantList = $("#participant-list").empty();
        data.forEach(participant => {
            participantList.append(`<li class="participant-item" data-id="${participant.participantID}">${participant.participantName}</li>`);
        });
    });
});

// Handle participant selection from the modal
$("#participant-list").on("click", ".participant-item", function () {
    const participantId = $(this).data("id");
    const participantName = $(this).text();

    // Set the recipient ID and display the started chat message
    $("#recipient-id").val(participantId);
    $("#chat-messages").html(`<div class="message">Started chat with ${participantName}</div>`);

    // Close the modal
    $("#new-chat-modal").fadeOut();
});

// Close Modal
$("#close-modal").on("click", function () {
    $("#new-chat-modal").fadeOut();
});

// Handle message sending
$("#chat-form").submit(function (e) {
    e.preventDefault();

    const recipientId = $("#recipient-id").val();  // Get the recipient ID
    const message = $("#message-input").val();     // Get the message from input field

    // Check if message is not empty
    if (message.trim() && recipientId) {
        // Send the message via AJAX
        $.post("/Chats/SendMessage", { recipientId, message }, function (data) {
            const newMessage = `
                <div class="message send">
                    <p>${message}</p>
                    <span class="time">${data.timeSent}</span>
                </div>
            `;
            // Append the new message to the chat window
            $("#chat-messages").append(newMessage);
            $("#message-input").val("");  // Clear the input field
            $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);  // Auto-scroll to the bottom
        });
    }
});

// Handle receiving messages through SignalR
connection.on("ReceiveMessage", (recipientId, message, timeSent, senderRole) => {
    // Only display the message if it's for the current recipient
    if (recipientId == $("#recipient-id").val()) {
        const messageClass = senderRole === "Patient" ? "receive" : "send";
        const newMessage = `
            <div class="message ${messageClass}">
                <p>${message}</p>
                <span class="time">${timeSent}</span>
            </div>
        `;
        // Append the received message to the chat window
        $("#chat-messages").append(newMessage);
        $("#chat-messages").scrollTop($("#chat-messages")[0].scrollHeight);  // Auto-scroll to the bottom
    }
});

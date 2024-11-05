// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function createDoctorCard(data) {
    const doctorCardContainer = document.getElementById('doctor-card-container');

    const card = document.createElement('div');
    card.className = 'ms-DocumentCard root-500';
    card.setAttribute('role', 'group');
    card.setAttribute('aria-label', 'Doctor Card');

    card.innerHTML = `
        <a href="@Url.Action("DoctorDetails", "Doctor", new { id = "<insert_id_here>" })" style="text-decoration: none; color: inherit;">
            <div class="root-501">
                <div class="ms-Image root-517" style="height: 150px;">
                    <img src="${data.ImageSrc}" role="presentation" alt="" class="ms-Image-image is-loaded ms-Image-image--centerCover ms-Image-image--portrait is-fadeIn image-518">
                </div>
                <i aria-hidden="true" class="cornerIcon-519"></i>
            </div>
            <div class="ms-DocumentCardDetails root-508">
                <div class="sc-fTABeZ sc-hRxedE hsqAaa dntOkG">
                    <span title="${data.Name}" class="css-509">${data.Name}</span>
                </div>
                <div class="sc-fTABeZ hsqAaa">
                    <span class="css-510">Degree: ${data.Degree}</span>
                </div>
                <div class="sc-fTABeZ hsqAaa">
                    <span class="css-511">Available: ${data.AvailabilityHours}</span>
                </div>
                <div class="sc-fTABeZ hsqAaa">
                    <span class="css-511">Hospital: ${data.Hospital}</span>
                </div>
            </div>
        </a>
    `;

    doctorCardContainer.appendChild(card);
}

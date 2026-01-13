function previewImage(event) {
    const preview = document.getElementById('preview');
    const file = event.target.files[0];

    if (file) {
        const imageUrl = URL.createObjectURL(file);
        preview.src = imageUrl; 
        preview.style.display = 'block'; 

        const currentImage = document.getElementById('currentImage');
        if (currentImage) {
            currentImage.style.display = 'none'; 
        }

        preview.onload = function () {
            URL.revokeObjectURL(imageUrl); 
        }
    }
}

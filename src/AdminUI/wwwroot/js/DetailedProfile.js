export async function setImageUsingStreaming (imageElementId, imageStream) {
    const arrayBuffer = await imageStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    document.getElementById(imageElementId).src = url;
}

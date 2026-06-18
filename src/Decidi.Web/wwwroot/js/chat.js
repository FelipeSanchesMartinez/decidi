export function scrollToBottom(elementId) {
    const el = document.getElementById(elementId);
    if (!el) return;
    el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
}

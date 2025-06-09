// Utility to convert camelCase or PascalCase to "Title Case"
export default function toLabelFromProperty(key: string) {
    return key
        .replace(/([A-Z])/g, " $1") // insert space before capital letters
        .replace(/^./, str => str.toUpperCase()) // capitalize first letter
        .trim();
}
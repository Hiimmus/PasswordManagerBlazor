function copy(password) {
	navigator.clipboard.writeText(password);
	alert("Copied the text: " + password);
}
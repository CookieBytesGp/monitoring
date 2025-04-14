export class StyleHelper {
  private styles: { [key: string]: { [key: string]: string } } = {};

  // Add or update a single style rule
  addStyle(className: string, cssProperties: { [key: string]: string }) {
    this.styles[className] = { ...this.styles[className], ...cssProperties };
  }
  updateCSS(currentCSS: string, selector: string, properties: { [key: string]: string }): string {
    // First parse existing CSS
    this.parseAndAddStyles(currentCSS);
    
    // Add or update the new styles
    this.addStyle(selector, properties);
    
    // Generate and return the updated CSS
    return this.generateCSS();
}
  // Parse a bulk CSS string and store styles
  parseAndAddStyles(cssString: string) {
    const regex = /([.#\w\d\[\]=:"\- ]+)\s*{\s*([^}]*)\s*}/g;
      
    let match;

    while ((match = regex.exec(cssString)) !== null) {
      const className = match[1].trim(); // Extract selector (e.g., '.CookieBox')
      const properties = match[2] // Extract properties
        .split(';')
        .filter((prop) => prop.trim() !== '')
        .reduce((acc: { [key: string]: string }, prop) => {
          const [key, value] = prop.split(':').map((s) => s.trim());
          acc[key] = value;
          return acc;
        }, {});

      this.addStyle(className, properties);
    }
  }

  // Get CSS properties for a specific class
  getStyle(className: string): { [key: string]: string } | undefined {
    return this.styles[className];
  }

  // Combine styles from multiple classes
  combineStyles(classNames: string[]): { [key: string]: string } {
    return classNames.reduce((combined, className) => {
      return { ...combined, ...this.styles[className] };
    }, {});
  }

  // Generate CSS string
  generateCSS(): string {
    return Object.entries(this.styles)
      .map(([className, cssProperties]) => {
        const propertiesString = Object.entries(cssProperties)
          .map(([key, value]) => `${key}: ${value};`)
          .join(' ');
        return `${className} { ${propertiesString} }`;
      })
      .join(' ');
  }

  // Clear all styles
  clearStyles() {
    this.styles = {};
  }
}

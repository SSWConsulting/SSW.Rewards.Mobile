export class BaseClient {
    public token: string = "";
    private readonly AUTH_HEADER = "Authorization";

    setAuthToken(token: string): void {
        this.token = token;
    }

    transformOptions(options: any): Promise<any> {
        options.headers[this.AUTH_HEADER] = `Bearer ${this.token}`;
        return Promise.resolve(options);
    }
}
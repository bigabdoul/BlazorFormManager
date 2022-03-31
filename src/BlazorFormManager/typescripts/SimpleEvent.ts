interface ISimpleEvent<T> {
    subscribe(handler: { (data?: T): void }): void;
    unsubscribe(handler: { (data?: T): void }): void;
}

class SimpleEvent<T> implements ISimpleEvent<T> {
    private delegates: { (data?: T): void; }[] = [];

    subscribe(handler: { (data?: T): void }): void {
        this.delegates.push(handler);
    }

    unsubscribe(handler: { (data?: T): void }): void {
        this.delegates = this.delegates.filter(h => h !== handler);
    }

    trigger(data?: T) {
        this.delegates.slice(0).forEach(d => d(data));
    }

    expose(): ISimpleEvent<T> {
        return this;
    }

    anySubscriber() {
        return this.delegates.length > 0;
    }
}

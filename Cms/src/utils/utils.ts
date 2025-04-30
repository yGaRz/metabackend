export const api_url = `${process.env.NEXT_PUBLIC_API_PROTOCOL || 'https'}://${
    process.env.NEXT_PUBLIC_API_HOST || 'localhost'
}:${process.env.NEXT_PUBLIC_API_PORT || 443}${process.env.NEXT_PUBLIC_API_BASE_PATH || ''}`;

export const getMetadataTitle = (titleName: string): string => {
    return `${titleName} | Admin dashboard for metacity`;
};

export const getDefaultDescription = () => {
    return `Metacity CMS system`;
};

export enum ToastColor {
    default,
    danger,
    success,
}

export const getToastStyle = (color: ToastColor = ToastColor.default) => {
    let hexColor;
    switch (color) {
        case ToastColor.default:
            hexColor = '#5750f1';
            break;
        case ToastColor.danger:
            hexColor = '#f44336';
            break;
        case ToastColor.success:
            hexColor = '#3FD97F';
            break;
    }
    return {
        style: {
            border: `1px solid ${hexColor}`,
            padding: '16px',
        },
    };
};

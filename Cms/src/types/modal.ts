export type ModalProps = {
    isOpen: boolean;
    onClose: () => void;
    callback?: () => void;
};

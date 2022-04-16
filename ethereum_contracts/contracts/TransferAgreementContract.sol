// SPDX-License-Identifier: MIT
pragma solidity >=0.4.22 <0.9.0;

contract TransferAgreementContract {
    mapping(string => Slot) private slots;
    
    struct Slot {
        address sender;
        address recipient;
        uint money;
    }

    modifier is_sender(string memory slot_id) {
        require(msg.sender == slots[slot_id].sender, "Only the sender has permission to transfer");
        _;
    }

    event Transfer(address indexed sender, address indexed recipient, string indexed slot_id, uint money);

    function store_in_block(string memory slot_id, address recipient) public payable {
        require(recipient != address(0), "Recipient's address does not exist");
        require(msg.value > 0, "Amount of ether must be greater than 0");
        slots[slot_id] = Slot(msg.sender, recipient, msg.value);
    }

    function transfer_to_recipient(string memory slot_id) public is_sender(slot_id) {
        Slot memory slot = slots[slot_id];
        payable(slot.recipient).transfer(slot.money);
        emit Transfer(slot.sender, slot.recipient, slot_id, slot.money);
    }
    function deny_transfer(string memory slot_id) public is_sender(slot_id) {
        Slot memory slot = slots[slot_id];
        payable(slot.sender).transfer(slot.money);
    }
}